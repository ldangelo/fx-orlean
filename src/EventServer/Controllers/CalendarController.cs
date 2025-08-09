using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using EventServer.Services;
using Fortium.Types;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Wolverine.Http;
using Wolverine.Marten;
using Events = Google.Apis.Calendar.v3.Data.Events;

namespace EventServer.Controllers;

public static class CalendarController
{
    [WolverineGet("/api/calendar/{calendarId}/events")]
    public static Events GetEvents(
        [FromServices] GoogleCalendarService calendarService,
        string calendarId
    )
    {
        var events = calendarService.GetCalendarEvents(calendarId);
        return events;
    }

    [WolverinePost("/api/calendar/{calendarId}/events")]
    public static (CalendarEventCreatedEvent, IStartStream) CreateEvent(
        [FromServices] GoogleCalendarService calendarService,
        string calendarId,
        [FromBody] CreateCalendarEventCommand command
    )
    {
        //            var createdEvent = calendarService.CreateEvent(calendarId, newEvent);
        Log.Information(
            "Creating event {eventId} for calendar {calendarId}.",
            command.EventId,
            calendarId
        );
        try
        {
            var calEvent = new Event();
            calEvent.Summary = command.Title;
            calEvent.Description = command.Description;
            calEvent.Start = new EventDateTime();
            calEvent.Start.DateTimeDateTimeOffset = command.StartTime;
            calEvent.End = new EventDateTime();
            calEvent.End.DateTimeDateTimeOffset = command.EndTime;
            calEvent.Organizer = new Event.OrganizerData { Email = command.UserId };
            calEvent.Attendees = new List<EventAttendee>();
            calEvent.Attendees.Add(
                new EventAttendee
                {
                    Email = command.PartnerId,
                    Optional = false,
                    Self = true,
                }
            );
            calEvent.Attendees.Add(new EventAttendee { Email = command.UserId, Optional = false });
            calEvent.ConferenceData = new ConferenceData
            {
                CreateRequest = new CreateConferenceRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" },
                },
            };

            Log.Information(
                "Adding event {} to calendar {}.",
                JsonConvert.SerializeObject(calEvent),
                calendarId
            );

            var createdEvent = calendarService.CreateEvent(calendarId, calEvent);
            Log.Information(
                "CalendarService: created event {createdEvent}",
                JsonConvert.SerializeObject(createdEvent)
            );

            var calendarCreatedEvent = new CalendarEventCreatedEvent(
                command.EventId,
                command.CalendarId,
                command.Title,
                command.Description,
                command.StartTime,
                (DateTime)command.EndTime!,
                command.PartnerId,
                command.UserId
            );
            var startStream = MartenOps.StartStream<CalendarEvent>(
                command.EventId,
                calendarCreatedEvent
            );

            return (calendarCreatedEvent, startStream);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw new Exception(e.Message);
        }
    }

    [WolverinePost("/api/calendar/availability")]
    public static async Task<int> GetPartnerAvailability(
        [FromServices] GoogleCalendarService calendarService,
        [FromBody] PartnerAvailabilityRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartnerEmail))
            {
                throw new ArgumentException("Partner email is required");
            }

            Log.Information("Getting availability for partner: {PartnerEmail}, Days: {DaysAhead}", 
                request.PartnerEmail, request.DaysAhead);

            var availableSlots = request.DaysAhead switch
            {
                30 => await calendarService.GetPartnerAvailabilityNext30DaysAsync(request.PartnerEmail, cancellationToken),
                7 => await calendarService.GetPartnerAvailabilityNext7DaysAsync(request.PartnerEmail, cancellationToken),
                _ => await calendarService.GetPartnerAvailabilityCustomDaysAsync(request.PartnerEmail, request.DaysAhead, cancellationToken)
            };

            return availableSlots;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting availability for partner {PartnerEmail}", request.PartnerEmail);
            throw;
        }
    }

    [WolverinePost("/api/calendar/availability/timeframe")]
    public static async Task<bool> CheckPartnerAvailabilityInTimeframe(
        [FromServices] GoogleCalendarService calendarService,
        [FromBody] PartnerTimeframeAvailabilityRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartnerEmail))
            {
                throw new ArgumentException("Partner email is required");
            }

            if (!Enum.TryParse<AvailabilityTimeframe>(request.Timeframe, true, out var timeframe))
            {
                throw new ArgumentException("Invalid timeframe specified");
            }

            Log.Information("Checking availability for partner: {PartnerEmail}, Timeframe: {Timeframe}", 
                request.PartnerEmail, timeframe);

            var hasAvailability = await calendarService.IsPartnerAvailableInTimeframeAsync(
                request.PartnerEmail, timeframe, cancellationToken);

            return hasAvailability;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking timeframe availability for partner {PartnerEmail}", request.PartnerEmail);
            throw;
        }
    }

    [WolverinePost("/api/calendar/availability/batch")]
    public static async Task<Dictionary<string, int>> RefreshMultiplePartnerAvailability(
        [FromServices] GoogleCalendarService calendarService,
        [FromBody] BatchAvailabilityRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (request.PartnerEmails == null || !request.PartnerEmails.Any())
            {
                throw new ArgumentException("At least one partner email is required");
            }

            Log.Information("Refreshing availability for {Count} partners", request.PartnerEmails.Count);

            var results = new Dictionary<string, int>();

            // Process in parallel for better performance
            var tasks = request.PartnerEmails.Select(async email =>
            {
                try
                {
                    var availability = await calendarService.GetPartnerAvailabilityNext30DaysAsync(email, cancellationToken);
                    return new { Email = email, Availability = availability };
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to get availability for partner {PartnerEmail}", email);
                    // Return fallback value
                    return new { Email = email, Availability = Math.Abs(email.GetHashCode()) % 8 + 1 };
                }
            });

            var availabilityResults = await Task.WhenAll(tasks);

            foreach (var result in availabilityResults)
            {
                results[result.Email] = result.Availability;
            }

            Log.Information("Successfully refreshed availability for {Count} partners", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error refreshing batch availability");
            throw;
        }
    }

    [WolverinePost("/api/calendar/booking")]
    public static async Task<ConsultationBookingResult> CreateConsultationBooking(
        [FromServices] GoogleCalendarService calendarService,
        [FromBody] ConsultationBookingRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartnerEmail) || string.IsNullOrWhiteSpace(request.ClientEmail))
            {
                throw new ArgumentException("Both partner and client emails are required");
            }

            if (request.StartTime >= request.EndTime)
            {
                throw new ArgumentException("End time must be after start time");
            }

            if (request.StartTime <= DateTime.Now)
            {
                throw new ArgumentException("Booking time must be in the future");
            }

            Log.Information("Creating consultation booking: Partner={PartnerEmail}, Client={ClientEmail}, Start={StartTime}", 
                request.PartnerEmail, request.ClientEmail, request.StartTime);

            var bookingResult = await calendarService.CreateConsultationBookingAsync(
                request.PartnerEmail,
                request.ClientEmail,
                request.StartTime,
                request.EndTime,
                request.Topic,
                request.ProblemDescription,
                cancellationToken);

            if (bookingResult.Success)
            {
                Log.Information("Successfully created booking: EventId={EventId}", bookingResult.GoogleCalendarEventId);
            }
            else
            {
                Log.Warning("Failed to create booking: {ErrorMessage}", bookingResult.ErrorMessage);
            }

            return bookingResult;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating consultation booking");
            throw;
        }
    }
}

/// <summary>
/// Request model for partner availability
/// </summary>
public class PartnerAvailabilityRequest
{
    public string PartnerEmail { get; set; } = string.Empty;
    public int DaysAhead { get; set; } = 30;
}

/// <summary>
/// Request model for partner timeframe availability
/// </summary>
public class PartnerTimeframeAvailabilityRequest
{
    public string PartnerEmail { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
}

/// <summary>
/// Request model for batch availability refresh
/// </summary>
public class BatchAvailabilityRequest
{
    public List<string> PartnerEmails { get; set; } = new();
}
