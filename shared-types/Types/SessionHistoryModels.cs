using Fortium.Types;

namespace Fortium.Types;

/// <summary>
/// Comprehensive session history model combining booking and session data
/// </summary>
public class SessionHistory
{
    public Guid BookingId { get; set; }
    public Guid ConferenceId { get; set; }
    public string ClientEmail { get; set; } = string.Empty;
    public string PartnerEmail { get; set; } = string.Empty;
    public string ConsultationTopic { get; set; } = string.Empty;
    public string ClientProblemDescription { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime BookingCompletedAt { get; set; }
    public SessionStatus Status { get; set; }
    public decimal SessionFee { get; set; }
    public decimal PartnerPayout { get; set; }
    public decimal PlatformFee { get; set; }
    public string GoogleMeetLink { get; set; } = string.Empty;
    public string GoogleCalendarEventId { get; set; } = string.Empty;
    
    // Session completion data
    public DateTime? SessionCompletedAt { get; set; }
    public string? SessionNotes { get; set; }
    public int? SessionRating { get; set; }
    public bool? PaymentCaptured { get; set; }
    public DateTime? PaymentCapturedAt { get; set; }
    public string? StripeChargeId { get; set; }
    
    // Computed properties
    public bool IsUpcoming => DateTime.UtcNow < StartTime;
    public bool IsInProgress => DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime && Status != SessionStatus.Completed;
    public bool IsCompleted => Status == SessionStatus.Completed || SessionCompletedAt.HasValue;
    public bool IsCancelled => Status == SessionStatus.Cancelled;
    public string DurationDisplay => (EndTime - StartTime).TotalMinutes + " minutes";
    public string StatusDisplay => Status switch
    {
        SessionStatus.Scheduled => "Scheduled",
        SessionStatus.InProgress => "In Progress", 
        SessionStatus.Completed => "Completed",
        SessionStatus.Cancelled => "Cancelled",
        SessionStatus.NoShow => "No Show",
        _ => "Unknown"
    };
}

/// <summary>
/// Status enumeration for sessions
/// </summary>
public enum SessionStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

/// <summary>
/// Filter criteria for session history queries
/// </summary>
public class SessionHistoryFilter
{
    public string? ClientEmail { get; set; }
    public string? PartnerEmail { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SessionStatus? Status { get; set; }
    public decimal? MinSessionFee { get; set; }
    public decimal? MaxSessionFee { get; set; }
    public string? SearchTerm { get; set; } // Search in topic or problem description
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public SessionHistorySortBy SortBy { get; set; } = SessionHistorySortBy.StartTimeDesc;
}

/// <summary>
/// Sort options for session history
/// </summary>
public enum SessionHistorySortBy
{
    StartTimeAsc,
    StartTimeDesc,
    SessionFeeAsc,
    SessionFeeDesc,
    StatusAsc,
    StatusDesc,
    BookingDateAsc,
    BookingDateDesc
}

/// <summary>
/// Paged result for session history queries
/// </summary>
public class SessionHistoryResult
{
    public List<SessionHistory> Sessions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Session statistics summary
/// </summary>
public class SessionStatistics
{
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageSessionFee { get; set; }
    public decimal TotalPartnerPayouts { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatedSessions { get; set; }
    
    // Time-based metrics
    public int SessionsThisMonth { get; set; }
    public int SessionsLastMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public double MonthOverMonthGrowth => SessionsLastMonth == 0 ? 0 : 
        Math.Round(((double)(SessionsThisMonth - SessionsLastMonth) / SessionsLastMonth) * 100, 1);
}