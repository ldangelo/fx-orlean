using System.Net;
using System.Net.Mail;
using System.Text;

namespace EventServer.Services;

/// <summary>
/// Service for sending booking confirmation and reminder emails
/// </summary>
public class EmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromAddress;
    private readonly string _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        // Get SMTP configuration from environment or appsettings
        var smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        var username = configuration["Email:Username"] ?? Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
        var password = configuration["Email:Password"] ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
        
        _fromAddress = configuration["Email:FromAddress"] ?? "noreply@fx-orleans.com";
        _fromName = configuration["Email:FromName"] ?? "FX-Orleans Platform";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Email configuration is missing. Please set SMTP_USERNAME and SMTP_PASSWORD environment variables or configure Email section in appsettings.");
        }

        _smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        _logger.LogInformation("Email service initialized with SMTP host: {SmtpHost}:{SmtpPort}", smtpHost, smtpPort);
    }

    /// <summary>
    /// Sends booking confirmation emails to both client and partner
    /// </summary>
    public async Task<BookingEmailResult> SendBookingConfirmationEmailsAsync(
        string clientEmail,
        string partnerEmail,
        string partnerName,
        string consultationTopic,
        string clientProblemDescription,
        DateTime sessionStartTime,
        DateTime sessionEndTime,
        string googleMeetLink,
        string googleCalendarLink = null,
        CancellationToken cancellationToken = default)
    {
        var result = new BookingEmailResult();
        
        try
        {
            // Send client confirmation email
            var clientEmailSent = await SendClientConfirmationEmailAsync(
                clientEmail, partnerName, consultationTopic, sessionStartTime, 
                sessionEndTime, googleMeetLink, googleCalendarLink, cancellationToken);
            result.ClientEmailSent = clientEmailSent;

            // Send partner notification email  
            var partnerEmailSent = await SendPartnerNotificationEmailAsync(
                partnerEmail, clientEmail, consultationTopic, clientProblemDescription,
                sessionStartTime, sessionEndTime, googleMeetLink, googleCalendarLink, cancellationToken);
            result.PartnerEmailSent = partnerEmailSent;

            result.Success = clientEmailSent && partnerEmailSent;
            _logger.LogInformation("Booking confirmation emails sent - Client: {ClientSent}, Partner: {PartnerSent}", 
                clientEmailSent, partnerEmailSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation emails");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Sends a booking confirmation email to the client
    /// </summary>
    private async Task<bool> SendClientConfirmationEmailAsync(
        string clientEmail,
        string partnerName,
        string consultationTopic,
        DateTime sessionStartTime,
        DateTime sessionEndTime,
        string googleMeetLink,
        string googleCalendarLink = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Consultation Confirmed: {consultationTopic}";
            var body = BuildClientConfirmationEmailBody(partnerName, consultationTopic, sessionStartTime, sessionEndTime, googleMeetLink, googleCalendarLink);

            using var message = new MailMessage(_fromAddress, clientEmail)
            {
                From = new MailAddress(_fromAddress, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            await _smtpClient.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Client confirmation email sent to {ClientEmail}", clientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send client confirmation email to {ClientEmail}", clientEmail);
            return false;
        }
    }

    /// <summary>
    /// Sends a booking notification email to the partner
    /// </summary>
    private async Task<bool> SendPartnerNotificationEmailAsync(
        string partnerEmail,
        string clientEmail,
        string consultationTopic,
        string clientProblemDescription,
        DateTime sessionStartTime,
        DateTime sessionEndTime,
        string googleMeetLink,
        string googleCalendarLink = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"New Consultation Booked: {consultationTopic}";
            var body = BuildPartnerNotificationEmailBody(clientEmail, consultationTopic, clientProblemDescription, 
                sessionStartTime, sessionEndTime, googleMeetLink, googleCalendarLink);

            using var message = new MailMessage(_fromAddress, partnerEmail)
            {
                From = new MailAddress(_fromAddress, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            await _smtpClient.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Partner notification email sent to {PartnerEmail}", partnerEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send partner notification email to {PartnerEmail}", partnerEmail);
            return false;
        }
    }

    /// <summary>
    /// Sends meeting reminder emails
    /// </summary>
    public async Task<bool> SendMeetingReminderAsync(
        List<string> recipients,
        string consultationTopic,
        DateTime sessionStartTime,
        string googleMeetLink,
        bool isOneHourReminder = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reminderType = isOneHourReminder ? "1 Hour" : "24 Hour";
            var subject = $"Reminder: {consultationTopic} starts {(isOneHourReminder ? "in 1 hour" : "tomorrow")}";
            var body = BuildReminderEmailBody(consultationTopic, sessionStartTime, googleMeetLink, isOneHourReminder);

            var tasks = recipients.Select(async email =>
            {
                try
                {
                    using var message = new MailMessage(_fromAddress, email)
                    {
                        From = new MailAddress(_fromAddress, _fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8
                    };

                    await _smtpClient.SendMailAsync(message, cancellationToken);
                    _logger.LogInformation("{ReminderType} reminder sent to {Email}", reminderType, email);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send {ReminderType} reminder to {Email}", reminderType, email);
                    return false;
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send meeting reminders");
            return false;
        }
    }

    private static string BuildClientConfirmationEmailBody(
        string partnerName, 
        string consultationTopic, 
        DateTime sessionStartTime, 
        DateTime sessionEndTime, 
        string googleMeetLink,
        string googleCalendarLink = null)
    {
        var timeZone = TimeZoneInfo.Local.DisplayName;
        var calendarLinkHtml = !string.IsNullOrEmpty(googleCalendarLink) 
            ? $"<p><a href=\"{googleCalendarLink}\" style=\"color: #2563eb; text-decoration: none;\">📅 Add to Google Calendar</a></p>" 
            : "";

        return $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                <h1 style="margin: 0; font-size: 28px;">Consultation Confirmed! ✅</h1>
                <p style="margin: 10px 0 0 0; font-size: 18px; opacity: 0.9;">You're all set for your expert consultation</p>
            </div>
            
            <div style="background: #f8fafc; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #e2e8f0; border-top: none;">
                <h2 style="color: #2d3748; margin-top: 0;">📋 Session Details</h2>
                
                <div style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #48bb78;">
                    <p><strong>Topic:</strong> {consultationTopic}</p>
                    <p><strong>Expert:</strong> {partnerName}</p>
                    <p><strong>Date & Time:</strong> {sessionStartTime:dddd, MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {sessionStartTime:h:mm tt} - {sessionEndTime:h:mm tt} ({timeZone})</p>
                    <p><strong>Duration:</strong> 60 minutes</p>
                </div>

                <h2 style="color: #2d3748;">🔗 Join Your Session</h2>
                <div style="text-align: center; margin: 25px 0;">
                    <a href="{googleMeetLink}" 
                       style="background: #48bb78; color: white; padding: 15px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 16px;">
                        🎥 Join Google Meet
                    </a>
                </div>
                
                {calendarLinkHtml}

                <h2 style="color: #2d3748;">📝 How to Prepare</h2>
                <ul style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0;">
                    <li>Review your problem statement and prepare specific questions</li>
                    <li>Gather any relevant documents or context you'd like to discuss</li>
                    <li>Test your Google Meet connection 5 minutes before the session</li>
                    <li>Prepare to take notes on recommendations and next steps</li>
                </ul>

                <div style="background: #e6fffa; border: 1px solid #81e6d9; border-radius: 6px; padding: 15px; margin: 20px 0;">
                    <p style="margin: 0; color: #234e52;"><strong>💡 Pro Tip:</strong> This is your dedicated time with a senior expert. Come prepared with your most pressing challenges to maximize the value of your consultation.</p>
                </div>

                <hr style="margin: 30px 0; border: none; border-top: 1px solid #e2e8f0;">
                
                <p style="text-align: center; color: #718096; font-size: 14px;">
                    Questions? Reply to this email or contact our support team.<br>
                    <strong>FX-Orleans</strong> - Expert Consultation Platform
                </p>
            </div>
        </body>
        </html>
        """;
    }

    private static string BuildPartnerNotificationEmailBody(
        string clientEmail,
        string consultationTopic,
        string clientProblemDescription,
        DateTime sessionStartTime,
        DateTime sessionEndTime,
        string googleMeetLink,
        string googleCalendarLink = null)
    {
        var timeZone = TimeZoneInfo.Local.DisplayName;
        var calendarLinkHtml = !string.IsNullOrEmpty(googleCalendarLink) 
            ? $"<p><a href=\"{googleCalendarLink}\" style=\"color: #2563eb; text-decoration: none;\">📅 View in Google Calendar</a></p>" 
            : "";

        return $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                <h1 style="margin: 0; font-size: 28px;">New Consultation Booked! 🎯</h1>
                <p style="margin: 10px 0 0 0; font-size: 18px; opacity: 0.9;">A client needs your expertise</p>
            </div>
            
            <div style="background: #f8fafc; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #e2e8f0; border-top: none;">
                <h2 style="color: #2d3748; margin-top: 0;">📋 Session Details</h2>
                
                <div style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #4299e1;">
                    <p><strong>Topic:</strong> {consultationTopic}</p>
                    <p><strong>Client:</strong> {clientEmail}</p>
                    <p><strong>Date & Time:</strong> {sessionStartTime:dddd, MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {sessionStartTime:h:mm tt} - {sessionEndTime:h:mm tt} ({timeZone})</p>
                    <p><strong>Duration:</strong> 60 minutes</p>
                    <p><strong>Session Fee:</strong> $800 (You'll receive $640)</p>
                </div>

                <h2 style="color: #2d3748;">🎯 Client Challenge</h2>
                <div style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #ed8936;">
                    <p style="margin: 0; font-style: italic; color: #4a5568;">"{clientProblemDescription}"</p>
                </div>

                <h2 style="color: #2d3748;">🔗 Session Access</h2>
                <div style="text-align: center; margin: 25px 0;">
                    <a href="{googleMeetLink}" 
                       style="background: #4299e1; color: white; padding: 15px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 16px;">
                        🎥 Join Google Meet
                    </a>
                </div>
                
                {calendarLinkHtml}

                <h2 style="color: #2d3748;">📝 Preparation Recommendations</h2>
                <ul style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0;">
                    <li>Review the client's specific challenge and context</li>
                    <li>Prepare actionable insights and recommendations</li>
                    <li>Consider relevant case studies or examples</li>
                    <li>Plan follow-up resources or next steps to recommend</li>
                </ul>

                <div style="background: #fef5e7; border: 1px solid #f6e05e; border-radius: 6px; padding: 15px; margin: 20px 0;">
                    <p style="margin: 0; color: #744210;"><strong>💰 Payment:</strong> Your $640 payout will be processed automatically after you mark the session as completed in your partner dashboard.</p>
                </div>

                <hr style="margin: 30px 0; border: none; border-top: 1px solid #e2e8f0;">
                
                <p style="text-align: center; color: #718096; font-size: 14px;">
                    Questions? Reply to this email or access your partner dashboard.<br>
                    <strong>FX-Orleans Partner Network</strong>
                </p>
            </div>
        </body>
        </html>
        """;
    }

    private static string BuildReminderEmailBody(
        string consultationTopic,
        DateTime sessionStartTime,
        string googleMeetLink,
        bool isOneHourReminder)
    {
        var reminderText = isOneHourReminder ? "in 1 hour" : "tomorrow";
        var urgencyColor = isOneHourReminder ? "#ed8936" : "#4299e1";
        var timeZone = TimeZoneInfo.Local.DisplayName;

        return $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background: {urgencyColor}; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                <h1 style="margin: 0; font-size: 28px;">⏰ Consultation Reminder</h1>
                <p style="margin: 10px 0 0 0; font-size: 18px; opacity: 0.9;">Your session starts {reminderText}</p>
            </div>
            
            <div style="background: #f8fafc; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #e2e8f0; border-top: none;">
                <div style="background: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid {urgencyColor};">
                    <p><strong>Topic:</strong> {consultationTopic}</p>
                    <p><strong>Time:</strong> {sessionStartTime:h:mm tt} ({timeZone})</p>
                    <p><strong>Date:</strong> {sessionStartTime:dddd, MMMM dd, yyyy}</p>
                </div>

                <div style="text-align: center; margin: 25px 0;">
                    <a href="{googleMeetLink}" 
                       style="background: {urgencyColor}; color: white; padding: 15px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 16px;">
                        🎥 Join Google Meet
                    </a>
                </div>

                <p style="text-align: center; color: #718096; font-size: 14px;">
                    <strong>FX-Orleans</strong> - Expert Consultation Platform
                </p>
            </div>
        </body>
        </html>
        """;
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}

/// <summary>
/// Result of sending booking confirmation emails
/// </summary>
public class BookingEmailResult
{
    public bool Success { get; set; }
    public bool ClientEmailSent { get; set; }
    public bool PartnerEmailSent { get; set; }
    public string? ErrorMessage { get; set; }
}