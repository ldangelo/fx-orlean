using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using EventServer.Services;
using Fortium.Types;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;

namespace EventServer.Controllers
{
    public static class CalendarController
    {
        [WolverineGet("/api/calendar/{calendarId}/events")]
        public static IActionResult GetEvents(
            [FromServices] GoogleCalendarService calendarService,
            string calendarId
        )
        {
            var events = calendarService.GetCalendarEvents(calendarId);
            return new OkObjectResult(events);
        }

        [WolverinePost("/api/calendar/{calendarId}/events")]
        public static (CalendarEventCreatedEvent, IStartStream) CreateEvent(
            [FromServices] GoogleCalendarService calendarService,
            string calendarId,
            [FromBody] CreateCalendarEventCommand command
        )
        {
            //            var createdEvent = calendarService.CreateEvent(calendarId, newEvent);
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
}
