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
            calEvent.Start.DateTime = command.StartTime;
            calEvent.End = new EventDateTime();
            calEvent.End.DateTime = command.EndTime;
            calEvent.Attendees = new List<EventAttendee>();
            calEvent.Attendees.Add(new EventAttendee { Email = command.UserId });

            Log.Information(
                "Adding event {} to calendar {}.",
                JsonConvert.SerializeObject(calEvent),
                calendarId
            );

            calendarService.CreateEvent(calendarId, calEvent);

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
}
