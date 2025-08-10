using Marten.Events.Aggregation;
using EventServer.Aggregates.VideoConference.Events;
using Fortium.Types;

namespace EventServer.Aggregates.VideoConference;

/// <summary>
/// Session history view model built from booking and session completion events
/// </summary>
public class SessionHistoryView
{
    public string Id { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public Guid ConferenceId { get; set; }
    public string ClientEmail { get; set; } = string.Empty;
    public string PartnerEmail { get; set; } = string.Empty;
    public string ConsultationTopic { get; set; } = string.Empty;
    public string ClientProblemDescription { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime BookingCompletedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public decimal SessionFee { get; set; }
    public decimal PartnerPayout { get; set; }
    public decimal PlatformFee { get; set; }
    public string GoogleMeetLink { get; set; } = string.Empty;
    public string GoogleCalendarEventId { get; set; } = string.Empty;
    
    // Session completion data
    public DateTime? SessionCompletedAt { get; set; }
    public string? SessionNotes { get; set; }
    public int? SessionRating { get; set; }
    public bool PaymentCaptured { get; set; }
    public DateTime? PaymentCapturedAt { get; set; }
    public string? StripeChargeId { get; set; }
}

/// <summary>
/// Projection that builds session history from booking and session events
/// </summary>
public class SessionHistoryProjection : SingleStreamProjection<SessionHistoryView, string>
{
    /// <summary>
    /// Create initial session history from booking completion
    /// </summary>
    public static SessionHistoryView Create(BookingCompletedEvent @event)
    {
        return new SessionHistoryView
        {
            Id = @event.BookingId.ToString(),
            BookingId = @event.BookingId,
            ConferenceId = @event.ConferenceId,
            ClientEmail = @event.ClientEmail,
            PartnerEmail = @event.PartnerEmail,
            ConsultationTopic = @event.ConsultationTopic,
            ClientProblemDescription = @event.ClientProblemDescription,
            StartTime = @event.StartTime,
            EndTime = @event.EndTime,
            BookingCompletedAt = @event.BookingCompletedAt,
            Status = SessionStatus.Scheduled,
            SessionFee = @event.SessionFee,
            PartnerPayout = @event.PartnerPayout,
            PlatformFee = @event.PlatformFee,
            GoogleMeetLink = @event.GoogleMeetLink,
            GoogleCalendarEventId = @event.GoogleCalendarEventId,
            PaymentCaptured = false
        };
    }

    /// <summary>
    /// Update session history when session is completed
    /// </summary>
    public static SessionHistoryView Apply(SessionCompletedEvent @event, SessionHistoryView view)
    {
        view.Status = SessionStatus.Completed;
        view.SessionCompletedAt = @event.ActualCompletionTime;
        view.SessionNotes = @event.SessionNotes;
        view.SessionRating = @event.SessionRating;
        return view;
    }

    /// <summary>
    /// Update session history when payment is captured
    /// </summary>
    public static SessionHistoryView Apply(PaymentCapturedEvent @event, SessionHistoryView view)
    {
        view.PaymentCaptured = true;
        view.PaymentCapturedAt = @event.PaymentCapturedAt;
        view.StripeChargeId = @event.StripeChargeId;
        return view;
    }
}

/// <summary>
/// Single-stream projection for aggregated session statistics by partner
/// </summary>
public class PartnerSessionStatsProjection : SingleStreamProjection<PartnerSessionStats, string>
{

    public static PartnerSessionStats Create(BookingCompletedEvent @event)
    {
        var isThisMonth = @event.BookingCompletedAt.Month == DateTime.UtcNow.Month && 
                         @event.BookingCompletedAt.Year == DateTime.UtcNow.Year;
        var isLastMonth = @event.BookingCompletedAt.Month == DateTime.UtcNow.AddMonths(-1).Month && 
                         @event.BookingCompletedAt.Year == DateTime.UtcNow.AddMonths(-1).Year;

        return new PartnerSessionStats
        {
            Id = @event.PartnerEmail,
            PartnerEmail = @event.PartnerEmail,
            TotalSessions = 1,
            UpcomingSessions = @event.StartTime > DateTime.UtcNow ? 1 : 0,
            TotalRevenue = @event.SessionFee,
            TotalPartnerPayouts = @event.PartnerPayout,
            TotalPlatformFees = @event.PlatformFee,
            SessionsThisMonth = isThisMonth ? 1 : 0,
            SessionsLastMonth = isLastMonth ? 1 : 0,
            RevenueThisMonth = isThisMonth ? @event.SessionFee : 0,
            RevenueLastMonth = isLastMonth ? @event.SessionFee : 0
        };
    }

    public static PartnerSessionStats Apply(BookingCompletedEvent @event, PartnerSessionStats stats)
    {
        var isThisMonth = @event.BookingCompletedAt.Month == DateTime.UtcNow.Month && 
                         @event.BookingCompletedAt.Year == DateTime.UtcNow.Year;
        var isLastMonth = @event.BookingCompletedAt.Month == DateTime.UtcNow.AddMonths(-1).Month && 
                         @event.BookingCompletedAt.Year == DateTime.UtcNow.AddMonths(-1).Year;

        stats.TotalSessions++;
        if (@event.StartTime > DateTime.UtcNow)
            stats.UpcomingSessions++;
            
        stats.TotalRevenue += @event.SessionFee;
        stats.TotalPartnerPayouts += @event.PartnerPayout;
        stats.TotalPlatformFees += @event.PlatformFee;
        
        if (isThisMonth)
        {
            stats.SessionsThisMonth++;
            stats.RevenueThisMonth += @event.SessionFee;
        }
        
        if (isLastMonth)
        {
            stats.SessionsLastMonth++;
            stats.RevenueLastMonth += @event.SessionFee;
        }

        return stats;
    }

    public static PartnerSessionStats Apply(SessionCompletedEvent @event, PartnerSessionStats stats)
    {
        stats.CompletedSessions++;
        stats.UpcomingSessions = Math.Max(0, stats.UpcomingSessions - 1); // Decrease upcoming if this was upcoming

        if (@event.SessionRating > 0)
        {
            // Calculate new average rating
            var totalRatingPoints = stats.AverageRating * stats.TotalRatedSessions + @event.SessionRating;
            stats.TotalRatedSessions++;
            stats.AverageRating = totalRatingPoints / stats.TotalRatedSessions;
        }

        return stats;
    }
}

/// <summary>
/// Partner session statistics view
/// </summary>
public class PartnerSessionStats
{
    public string Id { get; set; } = string.Empty;
    public string PartnerEmail { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPartnerPayouts { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatedSessions { get; set; }
    
    // Time-based metrics
    public int SessionsThisMonth { get; set; }
    public int SessionsLastMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    
    // Computed properties
    public decimal AverageSessionFee => TotalSessions > 0 ? TotalRevenue / TotalSessions : 0;
    public double CompletionRate => TotalSessions > 0 ? (double)CompletedSessions / TotalSessions * 100 : 0;
    public double MonthOverMonthGrowth => SessionsLastMonth == 0 ? 0 : 
        Math.Round(((double)(SessionsThisMonth - SessionsLastMonth) / SessionsLastMonth) * 100, 1);
}