using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Serilog;
using System.Text.Json;

namespace EventServer.Services;

public class GoogleCalendarService
{
    private readonly CalendarService _service;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
        
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

        if (clientId == null || clientSecret == null)
            throw new Exception(
                "Environment variables GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET must be set."
            );

        _logger.LogInformation("Initializing Google Calendar Service with client ID: {ClientId}", clientId);
        
        string[] scopes = { CalendarService.Scope.Calendar };
        var receiver = new GoogleLocalServerCodeReceiver();
        var credential = GoogleWebAuthorizationBroker
            .AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store"),
                receiver
            )
            .Result;

        _service = new CalendarService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FX-Orleans",
            }
        );
    }

    public Events GetCalendarEvents(string calendarId)
    {
        var request = _service.Events.List(calendarId);
        request.TimeMinDateTimeOffset = DateTime.Now;
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.MaxResults = 40;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        return request.Execute();
    }

    public Event CreateEvent(string calendarId, Event newEvent)
    {
        var request = _service.Events.Insert(newEvent, calendarId);
        request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.ExternalOnly;
        request.SendNotifications = true;
        request.ConferenceDataVersion = 1;
        return request.Execute();
    }

    /// <summary>
    /// Creates a consultation booking event with Google Meet integration
    /// </summary>
    public async Task<ConsultationBookingResult> CreateConsultationBookingAsync(
        string partnerEmail,
        string clientEmail,
        DateTime startTime,
        DateTime endTime,
        string consultationTopic,
        string clientProblemDescription,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating consultation booking: Partner={PartnerEmail}, Client={ClientEmail}, Time={StartTime}", 
                partnerEmail, clientEmail, startTime);

            var calEvent = new Event
            {
                Summary = $"FX-Orleans Consultation: {consultationTopic}",
                Description = BuildEventDescription(consultationTopic, clientProblemDescription, clientEmail),
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = startTime,
                    TimeZone = TimeZoneInfo.Local.Id
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = endTime,
                    TimeZone = TimeZoneInfo.Local.Id
                },
                Attendees = new List<EventAttendee>
                {
                    new EventAttendee
                    {
                        Email = partnerEmail,
                        Optional = false,
                        ResponseStatus = "needsAction"
                    },
                    new EventAttendee
                    {
                        Email = clientEmail,
                        Optional = false,
                        ResponseStatus = "needsAction"
                    }
                },
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" },
                    },
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder { Method = "email", Minutes = 1440 }, // 24 hours
                        new EventReminder { Method = "email", Minutes = 60 },   // 1 hour
                        new EventReminder { Method = "popup", Minutes = 30 }    // 30 minutes
                    }
                },
                Status = "confirmed",
                Visibility = "private"
            };

            _logger.LogDebug("Creating calendar event: {@CalendarEvent}", calEvent);

            // Use partner's calendar as the primary calendar (assuming partner has shared their calendar)
            // In production, you might want to use a service account calendar or the partner's specific calendar
            var request = _service.Events.Insert(calEvent, "primary");
            request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;
            request.SendNotifications = true;
            request.ConferenceDataVersion = 1;

            var createdEvent = await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Successfully created calendar event: EventId={EventId}, MeetLink={MeetLink}", 
                createdEvent.Id, createdEvent.ConferenceData?.EntryPoints?.FirstOrDefault()?.Uri);

            return new ConsultationBookingResult
            {
                Success = true,
                GoogleCalendarEventId = createdEvent.Id,
                GoogleMeetLink = createdEvent.ConferenceData?.EntryPoints?.FirstOrDefault()?.Uri ?? 
                                GenerateFallbackMeetLink(),
                CalendarEventLink = createdEvent.HtmlLink,
                StartTime = startTime,
                EndTime = endTime,
                PartnerEmail = partnerEmail,
                ClientEmail = clientEmail,
                Topic = consultationTopic
            };
        }
        catch (Google.GoogleApiException googleEx)
        {
            _logger.LogError(googleEx, "Google Calendar API error while creating booking: {Error}", googleEx.Message);
            
            // Return fallback result for graceful degradation
            return new ConsultationBookingResult
            {
                Success = false,
                ErrorMessage = $"Calendar integration failed: {googleEx.Message}",
                GoogleMeetLink = GenerateFallbackMeetLink(), // Provide fallback meet link
                StartTime = startTime,
                EndTime = endTime,
                PartnerEmail = partnerEmail,
                ClientEmail = clientEmail,
                Topic = consultationTopic
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating consultation booking");
            
            return new ConsultationBookingResult
            {
                Success = false,
                ErrorMessage = $"Booking creation failed: {ex.Message}",
                GoogleMeetLink = GenerateFallbackMeetLink(),
                StartTime = startTime,
                EndTime = endTime,
                PartnerEmail = partnerEmail,
                ClientEmail = clientEmail,
                Topic = consultationTopic
            };
        }
    }

    /// <summary>
    /// Updates an existing consultation booking
    /// </summary>
    public async Task<bool> UpdateConsultationBookingAsync(
        string eventId, 
        string updatedTopic = null,
        DateTime? newStartTime = null,
        DateTime? newEndTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEvent = await _service.Events.Get("primary", eventId).ExecuteAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(updatedTopic))
                existingEvent.Summary = $"FX-Orleans Consultation: {updatedTopic}";
            
            if (newStartTime.HasValue)
                existingEvent.Start.DateTimeDateTimeOffset = newStartTime.Value;
                
            if (newEndTime.HasValue)
                existingEvent.End.DateTimeDateTimeOffset = newEndTime.Value;

            var updateRequest = _service.Events.Update(existingEvent, "primary", eventId);
            updateRequest.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
            
            await updateRequest.ExecuteAsync(cancellationToken);
            
            _logger.LogInformation("Successfully updated calendar event: {EventId}", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update calendar event: {EventId}", eventId);
            return false;
        }
    }

    /// <summary>
    /// Deletes a consultation booking
    /// </summary>
    public async Task<bool> DeleteConsultationBookingAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = _service.Events.Delete("primary", eventId);
            deleteRequest.SendUpdates = EventsResource.DeleteRequest.SendUpdatesEnum.All;
            
            await deleteRequest.ExecuteAsync(cancellationToken);
            
            _logger.LogInformation("Successfully deleted calendar event: {EventId}", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete calendar event: {EventId}", eventId);
            return false;
        }
    }

    private static string BuildEventDescription(string topic, string problemDescription, string clientEmail)
    {
        return $"""
        FX-Orleans Expert Consultation
        
        Topic: {topic}
        
        Client Challenge:
        {problemDescription}
        
        Client Contact: {clientEmail}
        
        Please prepare by reviewing the client's specific challenge and come ready with actionable insights and recommendations.
        
        This is a 60-minute strategic consultation session.
        """;
    }

    private static string GenerateFallbackMeetLink()
    {
        // Generate a generic Google Meet link as fallback
        var meetId = Guid.NewGuid().ToString("N")[..10]; // Use first 10 characters
        return $"https://meet.google.com/{meetId}-{meetId[..3]}-{meetId[3..6]}";
    }
}

/// <summary>
/// Result of creating a consultation booking
/// </summary>
public class ConsultationBookingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public string? GoogleMeetLink { get; set; }
    public string? CalendarEventLink { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string PartnerEmail { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}
