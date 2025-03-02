using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using EventServer.Services;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine.Http;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class CalendarController
{
    [WolverineGet("/api/calendar/{calendarId}/events")]
    public static Google.Apis.Calendar.v3.Data.Events GetEvents(
        [FromServices] GoogleCalendarService calendarService,
        string calendarId
    )
    {
        var events = calendarService.GetCalendarEvents(calendarId);
        return events;
    }

    [WolverinePost("/api/calendar/{calendarId}/events")]
    public static (CalendarEventCreatedEvent, IStartStream) CreateEvent(
        string calendarId,
        [FromBody] CreateCalendarEventCommand command
    )
    {
        //            var createdEvent = calendarService.CreateEvent(calendarId, newEvent);
        Log.Information(
            "Creating event {eventId} for calendar {calendarId}.",
            command.eventId,
            calendarId
        );
        var calendarCreatedEvent = new CalendarEventCreatedEvent(
            command.eventId,
            command.calendarId,
            command.title,
            command.description,
            command.startTime,
            (DateTime)command.endTime,
            command.invitee
        );
        var startStream = MartenOps.StartStream<CalendarEvent>(
            command.eventId,
            calendarCreatedEvent
        );

        return (calendarCreatedEvent, startStream);
    }
}
