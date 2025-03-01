namespace EventServer.Aggregates.Calendar.Commands;

public record CreateCalendarEventCommand(
    string eventId,
    string calendarId,
    string title,
    string description,
    DateTime startTime,
    DateTime? endTime,
    string invitee
);
