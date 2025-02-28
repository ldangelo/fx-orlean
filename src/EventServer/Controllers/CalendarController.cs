using Microsoft.AspNetCore.Mvc;
using EventServer.Services;
using Google.Apis.Calendar.v3.Data;

namespace EventServer.Controllers
{
    public static class CalendarController
    {
        [WolverineGet("/api/calendar/{calendarId}/events")]
        public static IActionResult GetEvents([FromServices] CalendarService calendarService, string calendarId)
        {
            var events = calendarService.GetCalendarEvents(calendarId);
            return new OkObjectResult(events);
        }

        [WolverinePost("/api/calendar/{calendarId}/events")]
        public static IActionResult CreateEvent([FromServices] CalendarService calendarService, string calendarId, [FromBody] Event newEvent)
        {
            var createdEvent = calendarService.CreateEvent(calendarId, newEvent);
            return new CreatedAtActionResult(nameof(GetEvents), "Calendar", new { calendarId = calendarId }, createdEvent);
        }
    }
}
