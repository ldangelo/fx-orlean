using FxExpert.Blazor.Client.Models;
using Fortium.Types;

namespace FxExpert.Blazor.Client.Services;

/// <summary>
/// Interface for HTTP client service for calendar operations
/// </summary>
public interface ICalendarHttpService
{
    /// <summary>
    /// Gets partner availability for the next 30 days from the server
    /// </summary>
    /// <param name="partnerEmail">Email address of the partner</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of available consultation slots</returns>
    Task<int> GetPartnerAvailabilityNext30DaysAsync(string partnerEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a partner is available during a specific timeframe
    /// </summary>
    /// <param name="partnerEmail">Email address of the partner</param>
    /// <param name="timeframe">Availability timeframe to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if partner has availability during the timeframe</returns>
    Task<bool> IsPartnerAvailableInTimeframeAsync(
        string partnerEmail, 
        AvailabilityTimeframe timeframe, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes availability data for all partners
    /// This method can be used to update partner availability in the background
    /// </summary>
    /// <param name="partnerEmails">List of partner email addresses</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of partner email to availability count</returns>
    Task<Dictionary<string, int>> RefreshMultiplePartnerAvailabilityAsync(
        List<string> partnerEmails, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a consultation booking with Google Calendar integration
    /// </summary>
    /// <param name="request">Booking request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Booking result with Google Meet link and calendar details</returns>
    Task<ConsultationBookingResponse> CreateConsultationBookingAsync(
        ConsultationBookingRequest request, 
        CancellationToken cancellationToken = default);
}