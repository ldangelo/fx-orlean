namespace EventServer.Aggregates.Calendar.Commands;

public record CreateCalendarEventCommand(
    string eventId,
    string calendarId,
    string title,
    string description,
    DateTime startTime,
    DateTime? endTime,
    string partnerId, // Partner e-mail address
    string userId // User e-mail address
);
