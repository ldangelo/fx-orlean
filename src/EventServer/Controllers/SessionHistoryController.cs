using EventServer.Aggregates.VideoConference;
using Fortium.Types;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine.Http;

namespace EventServer.Controllers;

public static class SessionHistoryController
{
    /// <summary>
    /// Gets session history for a specific partner
    /// </summary>
    [WolverineGet("/api/sessions/partner/{partnerEmail}")]
    public static async Task<SessionHistoryResult> GetPartnerSessionHistoryAsync(
        [FromRoute] string partnerEmail,
        [FromServices] IQuerySession session,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SessionHistorySortBy sortBy = SessionHistorySortBy.StartTimeDesc)
    {
        Log.Information("Getting session history for partner: {PartnerEmail}", partnerEmail);

        var query = session.Query<SessionHistoryView>()
            .Where(s => s.PartnerEmail == partnerEmail);

        // Apply filters
        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => 
                s.ConsultationTopic.Contains(searchTerm) || 
                s.ClientProblemDescription.Contains(searchTerm));
        }

        // Apply sorting
        query = sortBy switch
        {
            SessionHistorySortBy.StartTimeAsc => query.OrderBy(s => s.StartTime),
            SessionHistorySortBy.StartTimeDesc => query.OrderByDescending(s => s.StartTime),
            SessionHistorySortBy.SessionFeeAsc => query.OrderBy(s => s.SessionFee),
            SessionHistorySortBy.SessionFeeDesc => query.OrderByDescending(s => s.SessionFee),
            SessionHistorySortBy.StatusAsc => query.OrderBy(s => s.Status),
            SessionHistorySortBy.StatusDesc => query.OrderByDescending(s => s.Status),
            SessionHistorySortBy.BookingDateAsc => query.OrderBy(s => s.BookingCompletedAt),
            SessionHistorySortBy.BookingDateDesc => query.OrderByDescending(s => s.BookingCompletedAt),
            _ => query.OrderByDescending(s => s.StartTime)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Convert to SessionHistory models
        var sessionHistory = sessions.Select(s => new SessionHistory
        {
            BookingId = s.BookingId,
            ConferenceId = s.ConferenceId,
            ClientEmail = s.ClientEmail,
            PartnerEmail = s.PartnerEmail,
            ConsultationTopic = s.ConsultationTopic,
            ClientProblemDescription = s.ClientProblemDescription,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BookingCompletedAt = s.BookingCompletedAt,
            Status = s.Status,
            SessionFee = s.SessionFee,
            PartnerPayout = s.PartnerPayout,
            PlatformFee = s.PlatformFee,
            GoogleMeetLink = s.GoogleMeetLink,
            GoogleCalendarEventId = s.GoogleCalendarEventId,
            SessionCompletedAt = s.SessionCompletedAt,
            SessionNotes = s.SessionNotes,
            SessionRating = s.SessionRating,
            PaymentCaptured = s.PaymentCaptured,
            PaymentCapturedAt = s.PaymentCapturedAt,
            StripeChargeId = s.StripeChargeId
        }).ToList();

        return new SessionHistoryResult
        {
            Sessions = sessionHistory,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets session history for a specific client
    /// </summary>
    [WolverineGet("/api/sessions/client/{clientEmail}")]
    public static async Task<SessionHistoryResult> GetClientSessionHistoryAsync(
        [FromRoute] string clientEmail,
        [FromServices] IQuerySession session,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SessionHistorySortBy sortBy = SessionHistorySortBy.StartTimeDesc)
    {
        Log.Information("Getting session history for client: {ClientEmail}", clientEmail);

        var query = session.Query<SessionHistoryView>()
            .Where(s => s.ClientEmail == clientEmail);

        // Apply filters (same logic as partner history)
        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => 
                s.ConsultationTopic.Contains(searchTerm) || 
                s.ClientProblemDescription.Contains(searchTerm));
        }

        // Apply sorting
        query = sortBy switch
        {
            SessionHistorySortBy.StartTimeAsc => query.OrderBy(s => s.StartTime),
            SessionHistorySortBy.StartTimeDesc => query.OrderByDescending(s => s.StartTime),
            SessionHistorySortBy.SessionFeeAsc => query.OrderBy(s => s.SessionFee),
            SessionHistorySortBy.SessionFeeDesc => query.OrderByDescending(s => s.SessionFee),
            SessionHistorySortBy.StatusAsc => query.OrderBy(s => s.Status),
            SessionHistorySortBy.StatusDesc => query.OrderByDescending(s => s.Status),
            SessionHistorySortBy.BookingDateAsc => query.OrderBy(s => s.BookingCompletedAt),
            SessionHistorySortBy.BookingDateDesc => query.OrderByDescending(s => s.BookingCompletedAt),
            _ => query.OrderByDescending(s => s.StartTime)
        };

        // Get total count and apply pagination
        var totalCount = await query.CountAsync();
        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Convert to SessionHistory models
        var sessionHistory = sessions.Select(s => new SessionHistory
        {
            BookingId = s.BookingId,
            ConferenceId = s.ConferenceId,
            ClientEmail = s.ClientEmail,
            PartnerEmail = s.PartnerEmail,
            ConsultationTopic = s.ConsultationTopic,
            ClientProblemDescription = s.ClientProblemDescription,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BookingCompletedAt = s.BookingCompletedAt,
            Status = s.Status,
            SessionFee = s.SessionFee,
            PartnerPayout = s.PartnerPayout,
            PlatformFee = s.PlatformFee,
            GoogleMeetLink = s.GoogleMeetLink,
            GoogleCalendarEventId = s.GoogleCalendarEventId,
            SessionCompletedAt = s.SessionCompletedAt,
            SessionNotes = s.SessionNotes,
            SessionRating = s.SessionRating,
            PaymentCaptured = s.PaymentCaptured,
            PaymentCapturedAt = s.PaymentCapturedAt,
            StripeChargeId = s.StripeChargeId
        }).ToList();

        return new SessionHistoryResult
        {
            Sessions = sessionHistory,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets session statistics for a specific partner
    /// </summary>
    [WolverineGet("/api/sessions/partner/{partnerEmail}/stats")]
    public static async Task<SessionStatistics> GetPartnerSessionStatsAsync(
        [FromRoute] string partnerEmail,
        [FromServices] IQuerySession session)
    {
        Log.Information("Getting session statistics for partner: {PartnerEmail}", partnerEmail);

        // Get partner stats from projection
        var partnerStats = await session.LoadAsync<PartnerSessionStats>(partnerEmail);
        
        if (partnerStats == null)
        {
            return new SessionStatistics { TotalSessions = 0 };
        }

        return new SessionStatistics
        {
            TotalSessions = partnerStats.TotalSessions,
            CompletedSessions = partnerStats.CompletedSessions,
            CancelledSessions = partnerStats.CancelledSessions,
            UpcomingSessions = partnerStats.UpcomingSessions,
            TotalRevenue = partnerStats.TotalRevenue,
            AverageSessionFee = partnerStats.AverageSessionFee,
            TotalPartnerPayouts = partnerStats.TotalPartnerPayouts,
            TotalPlatformFees = partnerStats.TotalPlatformFees,
            AverageRating = partnerStats.AverageRating,
            TotalRatedSessions = partnerStats.TotalRatedSessions,
            SessionsThisMonth = partnerStats.SessionsThisMonth,
            SessionsLastMonth = partnerStats.SessionsLastMonth,
            RevenueThisMonth = partnerStats.RevenueThisMonth,
            RevenueLastMonth = partnerStats.RevenueLastMonth
            // MonthOverMonthGrowth is computed automatically from SessionsThisMonth and SessionsLastMonth
        };
    }

    /// <summary>
    /// Gets session statistics for a specific client
    /// </summary>
    [WolverineGet("/api/sessions/client/{clientEmail}/stats")]
    public static async Task<SessionStatistics> GetClientSessionStatsAsync(
        [FromRoute] string clientEmail,
        [FromServices] IQuerySession session)
    {
        Log.Information("Getting session statistics for client: {ClientEmail}", clientEmail);

        // Calculate client stats from session history
        var sessions = await session.Query<SessionHistoryView>()
            .Where(s => s.ClientEmail == clientEmail)
            .ToListAsync();

        if (!sessions.Any())
        {
            return new SessionStatistics { TotalSessions = 0 };
        }

        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();
        var ratedSessions = completedSessions.Where(s => s.SessionRating.HasValue && s.SessionRating > 0).ToList();

        return new SessionStatistics
        {
            TotalSessions = sessions.Count,
            CompletedSessions = completedSessions.Count,
            CancelledSessions = sessions.Count(s => s.Status == SessionStatus.Cancelled),
            UpcomingSessions = sessions.Count(s => s.StartTime > now),
            TotalRevenue = sessions.Sum(s => s.SessionFee),
            AverageSessionFee = sessions.Any() ? sessions.Average(s => s.SessionFee) : 0,
            TotalPartnerPayouts = sessions.Sum(s => s.PartnerPayout),
            TotalPlatformFees = sessions.Sum(s => s.PlatformFee),
            AverageRating = ratedSessions.Any() ? ratedSessions.Average(s => s.SessionRating ?? 0) : 0,
            TotalRatedSessions = ratedSessions.Count,
            SessionsThisMonth = sessions.Count(s => s.BookingCompletedAt >= thisMonth),
            SessionsLastMonth = sessions.Count(s => s.BookingCompletedAt >= lastMonth && s.BookingCompletedAt < thisMonth),
            RevenueThisMonth = sessions.Where(s => s.BookingCompletedAt >= thisMonth).Sum(s => s.SessionFee),
            RevenueLastMonth = sessions.Where(s => s.BookingCompletedAt >= lastMonth && s.BookingCompletedAt < thisMonth).Sum(s => s.SessionFee)
            // MonthOverMonthGrowth is computed automatically from SessionsThisMonth and SessionsLastMonth
        };
    }

    /// <summary>
    /// Gets details for a specific session
    /// </summary>
    [WolverineGet("/api/sessions/{bookingId:guid}")]
    public static async Task<IResult> GetSessionDetailsAsync(
        [FromRoute] Guid bookingId,
        [FromServices] IQuerySession session)
    {
        Log.Information("Getting session details for booking: {BookingId}", bookingId);

        var sessionView = await session.LoadAsync<SessionHistoryView>(bookingId.ToString());
        
        if (sessionView == null)
        {
            Log.Warning("Session not found for booking: {BookingId}", bookingId);
            return Results.NotFound();
        }

        var sessionHistory = new SessionHistory
        {
            BookingId = sessionView.BookingId,
            ConferenceId = sessionView.ConferenceId,
            ClientEmail = sessionView.ClientEmail,
            PartnerEmail = sessionView.PartnerEmail,
            ConsultationTopic = sessionView.ConsultationTopic,
            ClientProblemDescription = sessionView.ClientProblemDescription,
            StartTime = sessionView.StartTime,
            EndTime = sessionView.EndTime,
            BookingCompletedAt = sessionView.BookingCompletedAt,
            Status = sessionView.Status,
            SessionFee = sessionView.SessionFee,
            PartnerPayout = sessionView.PartnerPayout,
            PlatformFee = sessionView.PlatformFee,
            GoogleMeetLink = sessionView.GoogleMeetLink,
            GoogleCalendarEventId = sessionView.GoogleCalendarEventId,
            SessionCompletedAt = sessionView.SessionCompletedAt,
            SessionNotes = sessionView.SessionNotes,
            SessionRating = sessionView.SessionRating,
            PaymentCaptured = sessionView.PaymentCaptured,
            PaymentCapturedAt = sessionView.PaymentCapturedAt,
            StripeChargeId = sessionView.StripeChargeId
        };

        return Results.Ok(sessionHistory);
    }

    /// <summary>
    /// Gets upcoming sessions for a partner (next 7 days)
    /// </summary>
    [WolverineGet("/api/sessions/partner/{partnerEmail}/upcoming")]
    public static async Task<List<SessionHistory>> GetPartnerUpcomingSessionsAsync(
        [FromRoute] string partnerEmail,
        [FromServices] IQuerySession session)
    {
        Log.Information("Getting upcoming sessions for partner: {PartnerEmail}", partnerEmail);

        var oneWeekFromNow = DateTime.UtcNow.AddDays(7);
        var sessions = await session.Query<SessionHistoryView>()
            .Where(s => s.PartnerEmail == partnerEmail && 
                       s.StartTime >= DateTime.UtcNow && 
                       s.StartTime <= oneWeekFromNow &&
                       s.Status == SessionStatus.Scheduled)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => new SessionHistory
        {
            BookingId = s.BookingId,
            ConferenceId = s.ConferenceId,
            ClientEmail = s.ClientEmail,
            PartnerEmail = s.PartnerEmail,
            ConsultationTopic = s.ConsultationTopic,
            ClientProblemDescription = s.ClientProblemDescription,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BookingCompletedAt = s.BookingCompletedAt,
            Status = s.Status,
            SessionFee = s.SessionFee,
            PartnerPayout = s.PartnerPayout,
            PlatformFee = s.PlatformFee,
            GoogleMeetLink = s.GoogleMeetLink,
            GoogleCalendarEventId = s.GoogleCalendarEventId
        }).ToList();
    }

    /// <summary>
    /// Gets upcoming sessions for a client (next 30 days)
    /// </summary>
    [WolverineGet("/api/sessions/client/{clientEmail}/upcoming")]
    public static async Task<List<SessionHistory>> GetClientUpcomingSessionsAsync(
        [FromRoute] string clientEmail,
        [FromServices] IQuerySession session)
    {
        Log.Information("Getting upcoming sessions for client: {ClientEmail}", clientEmail);

        var oneMonthFromNow = DateTime.UtcNow.AddDays(30);
        var sessions = await session.Query<SessionHistoryView>()
            .Where(s => s.ClientEmail == clientEmail && 
                       s.StartTime >= DateTime.UtcNow && 
                       s.StartTime <= oneMonthFromNow &&
                       s.Status == SessionStatus.Scheduled)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => new SessionHistory
        {
            BookingId = s.BookingId,
            ConferenceId = s.ConferenceId,
            ClientEmail = s.ClientEmail,
            PartnerEmail = s.PartnerEmail,
            ConsultationTopic = s.ConsultationTopic,
            ClientProblemDescription = s.ClientProblemDescription,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BookingCompletedAt = s.BookingCompletedAt,
            Status = s.Status,
            SessionFee = s.SessionFee,
            PartnerPayout = s.PartnerPayout,
            PlatformFee = s.PlatformFee,
            GoogleMeetLink = s.GoogleMeetLink,
            GoogleCalendarEventId = s.GoogleCalendarEventId
        }).ToList();
    }
}