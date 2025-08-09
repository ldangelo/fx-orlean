using Fortium.Types;

namespace EventServer.Aggregates.VideoConference.Events;

public interface IVideoConferenceEvent
{
}

[Serializable]
public record VideoConferenceCreatedEvent(
    Guid ConferenceId,
    DateTime StartTime,
    DateTime EndTime,
    string UserId,
    string PartnerId,
    RateInformation RateInformation
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when a booking is completed with all integrations (payment, calendar, notifications)
/// </summary>
[Serializable]
public record BookingCompletedEvent(
    Guid BookingId,
    Guid ConferenceId,
    string ClientEmail,
    string PartnerEmail,
    DateTime StartTime,
    DateTime EndTime,
    string ConsultationTopic,
    string ClientProblemDescription,
    string PaymentIntentId,
    string GoogleCalendarEventId,
    string GoogleMeetLink,
    decimal SessionFee,
    decimal PartnerPayout,
    decimal PlatformFee,
    DateTime BookingCompletedAt
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when Google Calendar integration succeeds for a booking
/// </summary>
[Serializable]
public record CalendarIntegrationCompletedEvent(
    Guid BookingId,
    string GoogleCalendarEventId,
    string GoogleMeetLink,
    string CalendarEventLink,
    List<string> AttendeeEmails,
    DateTime CalendarEventCreatedAt
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when booking confirmation emails are sent
/// </summary>
[Serializable]
public record BookingConfirmationEmailsSentEvent(
    Guid BookingId,
    string ClientEmail,
    string PartnerEmail,
    bool ClientEmailSent,
    bool PartnerEmailSent,
    DateTime EmailsSentAt
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when meeting reminders are scheduled
/// </summary>
[Serializable]
public record MeetingRemindersScheduledEvent(
    Guid BookingId,
    DateTime TwentyFourHourReminderScheduled,
    DateTime OneHourReminderScheduled,
    List<string> ReminderRecipients
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when a consultation session is marked as completed by the partner
/// </summary>
[Serializable]
public record SessionCompletedEvent(
    Guid ConferenceId,
    Guid BookingId,
    string PartnerEmail,
    string ClientEmail,
    DateTime SessionStartTime,
    DateTime SessionEndTime,
    DateTime ActualCompletionTime,
    string? SessionNotes,
    int SessionRating,
    bool PaymentCaptureRequested
) : IVideoConferenceEvent;

/// <summary>
/// Event fired when payment is successfully captured after session completion
/// </summary>
[Serializable]
public record PaymentCapturedEvent(
    Guid BookingId,
    string PaymentIntentId,
    decimal CapturedAmount,
    decimal PartnerPayout,
    decimal PlatformFee,
    DateTime PaymentCapturedAt,
    string StripeChargeId
) : IVideoConferenceEvent;
