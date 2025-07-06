namespace EventServer.Aggregates.Calendar.Events;

public record CalendarEventCreatedEvent(
    string EventId,
    string CalendarId,
    string Title,
    string Description,
    DateTime Start,
    DateTime End,
    string PartnerId,
    string UserId
);
