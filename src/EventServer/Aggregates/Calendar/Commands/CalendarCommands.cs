namespace EventServer.Aggregates.Calendar.Commands;

public record CreateCalendarEventCommand(
    string EventId,
    string CalendarId,
    string Title,
    string Description,
    DateTime StartTime,
    DateTime? EndTime,
    string PartnerId, // Partner e-mail address
    string UserId // User e-mail address
);
