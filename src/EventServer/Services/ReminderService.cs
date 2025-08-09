using EventServer.Aggregates.VideoConference.Events;
using EventServer.Services;

namespace EventServer.Services;

/// <summary>
/// Background service to handle scheduled meeting reminders
/// </summary>
public class ReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderService> _logger;
    private readonly List<ScheduledReminder> _scheduledReminders = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ReminderService(IServiceProvider serviceProvider, ILogger<ReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRemindersAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Reminder service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reminder service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
            }
        }
    }

    /// <summary>
    /// Schedules reminders for a booking
    /// </summary>
    public async Task ScheduleRemindersAsync(
        Guid bookingId, 
        DateTime sessionStartTime, 
        string consultationTopic,
        string googleMeetLink,
        List<string> recipients)
    {
        await _semaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            
            // Schedule 24-hour reminder
            var twentyFourHourReminderTime = sessionStartTime.AddHours(-24);
            if (twentyFourHourReminderTime > now)
            {
                var twentyFourHourReminder = new ScheduledReminder
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    ScheduledTime = twentyFourHourReminderTime,
                    Recipients = recipients,
                    ConsultationTopic = consultationTopic,
                    SessionStartTime = sessionStartTime,
                    GoogleMeetLink = googleMeetLink,
                    IsOneHourReminder = false,
                    IsProcessed = false
                };
                
                _scheduledReminders.Add(twentyFourHourReminder);
                _logger.LogInformation("Scheduled 24-hour reminder for BookingId: {BookingId} at {ReminderTime}", 
                    bookingId, twentyFourHourReminderTime);
            }

            // Schedule 1-hour reminder
            var oneHourReminderTime = sessionStartTime.AddHours(-1);
            if (oneHourReminderTime > now)
            {
                var oneHourReminder = new ScheduledReminder
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    ScheduledTime = oneHourReminderTime,
                    Recipients = recipients,
                    ConsultationTopic = consultationTopic,
                    SessionStartTime = sessionStartTime,
                    GoogleMeetLink = googleMeetLink,
                    IsOneHourReminder = true,
                    IsProcessed = false
                };
                
                _scheduledReminders.Add(oneHourReminder);
                _logger.LogInformation("Scheduled 1-hour reminder for BookingId: {BookingId} at {ReminderTime}", 
                    bookingId, oneHourReminderTime);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Processes due reminders
    /// </summary>
    private async Task ProcessDueRemindersAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var dueReminders = _scheduledReminders
                .Where(r => !r.IsProcessed && r.ScheduledTime <= now)
                .ToList();

            if (!dueReminders.Any())
                return;

            _logger.LogInformation("Processing {Count} due reminders", dueReminders.Count);

            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            foreach (var reminder in dueReminders)
            {
                try
                {
                    var emailSent = await emailService.SendMeetingReminderAsync(
                        reminder.Recipients,
                        reminder.ConsultationTopic,
                        reminder.SessionStartTime,
                        reminder.GoogleMeetLink,
                        reminder.IsOneHourReminder,
                        cancellationToken);

                    reminder.IsProcessed = true;
                    reminder.ProcessedAt = DateTime.UtcNow;
                    reminder.EmailSent = emailSent;

                    _logger.LogInformation(
                        "Processed {ReminderType} reminder for BookingId: {BookingId}, EmailSent: {EmailSent}",
                        reminder.IsOneHourReminder ? "1-hour" : "24-hour",
                        reminder.BookingId,
                        emailSent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process reminder for BookingId: {BookingId}", reminder.BookingId);
                    reminder.IsProcessed = true; // Mark as processed to avoid retry loop
                    reminder.ProcessedAt = DateTime.UtcNow;
                    reminder.EmailSent = false;
                }
            }

            // Clean up old processed reminders (older than 7 days)
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var remindersToRemove = _scheduledReminders
                .Where(r => r.IsProcessed && r.ProcessedAt.HasValue && r.ProcessedAt < cutoffDate)
                .ToList();

            foreach (var oldReminder in remindersToRemove)
            {
                _scheduledReminders.Remove(oldReminder);
            }

            if (remindersToRemove.Any())
            {
                _logger.LogInformation("Cleaned up {Count} old processed reminders", remindersToRemove.Count);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Cancels reminders for a booking (used when booking is cancelled)
    /// </summary>
    public async Task CancelRemindersAsync(Guid bookingId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var remindersToCancel = _scheduledReminders
                .Where(r => r.BookingId == bookingId && !r.IsProcessed)
                .ToList();

            foreach (var reminder in remindersToCancel)
            {
                reminder.IsProcessed = true;
                reminder.ProcessedAt = DateTime.UtcNow;
                reminder.EmailSent = false;
            }

            _logger.LogInformation("Cancelled {Count} reminders for BookingId: {BookingId}", 
                remindersToCancel.Count, bookingId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets statistics about scheduled and processed reminders
    /// </summary>
    public async Task<ReminderServiceStats> GetStatsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            return new ReminderServiceStats
            {
                TotalScheduled = _scheduledReminders.Count,
                Processed = _scheduledReminders.Count(r => r.IsProcessed),
                Pending = _scheduledReminders.Count(r => !r.IsProcessed),
                Due = _scheduledReminders.Count(r => !r.IsProcessed && r.ScheduledTime <= now),
                SuccessfulEmails = _scheduledReminders.Count(r => r.IsProcessed && r.EmailSent),
                FailedEmails = _scheduledReminders.Count(r => r.IsProcessed && !r.EmailSent)
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override void Dispose()
    {
        _semaphore?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Represents a scheduled reminder
/// </summary>
public class ScheduledReminder
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public List<string> Recipients { get; set; } = new();
    public string ConsultationTopic { get; set; } = string.Empty;
    public DateTime SessionStartTime { get; set; }
    public string GoogleMeetLink { get; set; } = string.Empty;
    public bool IsOneHourReminder { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool EmailSent { get; set; }
}

/// <summary>
/// Statistics about the reminder service
/// </summary>
public class ReminderServiceStats
{
    public int TotalScheduled { get; set; }
    public int Processed { get; set; }
    public int Pending { get; set; }
    public int Due { get; set; }
    public int SuccessfulEmails { get; set; }
    public int FailedEmails { get; set; }
}