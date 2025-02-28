using Microsoft.AspNetCore.Mvc;
using EventServer.Services;
using Google.Apis.Calendar.v3.Data;

namespace EventServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarService _calendarService;

        public CalendarController(CalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet("{calendarId}/events")]
        public IActionResult GetEvents(string calendarId)
        {
            var events = _calendarService.GetCalendarEvents(calendarId);
            return Ok(events);
        }

        [HttpPost("{calendarId}/events")]
        public IActionResult CreateEvent(string calendarId, [FromBody] Event newEvent)
        {
            var createdEvent = _calendarService.CreateEvent(calendarId, newEvent);
            return CreatedAtAction(nameof(GetEvents), new { calendarId = calendarId }, createdEvent);
        }
    }
}
