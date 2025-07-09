using EventServer.Aggregates.Calendar.Events;
using Fortium.Types;
using Marten.Events.Aggregation;

namespace EventServer.Aggregates.Calendar;

public class CalendarEventProjection : SingleStreamProjection<CalendarEvent, string>
{
    public static CalendarEvent Apply(CalendarEventCreatedEvent @event, CalendarEvent calendarEvent)
    {
        calendarEvent.CalendarEventId = @event.EventId;
        calendarEvent.CalendarId = @event.CalendarId;
        calendarEvent.Description = @event.Description;
        calendarEvent.Start = @event.Start;
        calendarEvent.End = @event.End;
        calendarEvent.PartnerId = @event.PartnerId;
        calendarEvent.UserId = @event.UserId;
        calendarEvent.CreateDate = DateTime.Now;

        return calendarEvent;
    }
}
