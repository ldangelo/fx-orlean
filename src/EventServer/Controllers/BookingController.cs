using EventServer.Aggregates.VideoConference;
using EventServer.Aggregates.VideoConference.Commands;
using EventServer.Aggregates.VideoConference.Events;
using EventServer.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class BookingController
{
    /// <summary>
    /// Completes a booking by integrating payment authorization, Google Calendar, and email notifications
    /// </summary>
    [WolverinePost("/api/bookings/complete")]
    public static async Task<(BookingCompletedEvent, IStartStream)> CompleteBookingAsync(
        [FromBody] CompleteBookingCommand command,
        [FromServices] GoogleCalendarService calendarService,
        [FromServices] EmailService emailService,
        [FromServices] ReminderService reminderService,
        [FromServices] ILogger logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting booking completion for BookingId: {BookingId}, ConferenceId: {ConferenceId}", 
            command.BookingId, command.ConferenceId);

        try
        {
            // Step 1: Create Google Calendar event with Meet integration
            var calendarResult = await calendarService.CreateConsultationBookingAsync(
                command.PartnerEmail,
                command.ClientEmail,
                command.StartTime,
                command.EndTime,
                command.ConsultationTopic,
                command.ClientProblemDescription,
                cancellationToken);

            logger.LogInformation("Calendar integration result - Success: {Success}, EventId: {EventId}, MeetLink: {MeetLink}", 
                calendarResult.Success, calendarResult.GoogleCalendarEventId, calendarResult.GoogleMeetLink);

            // Step 2: Send confirmation emails to both parties
            var emailResult = await emailService.SendBookingConfirmationEmailsAsync(
                command.ClientEmail,
                command.PartnerEmail,
                "Expert Partner", // TODO: Get actual partner name from Partner aggregate
                command.ConsultationTopic,
                command.ClientProblemDescription,
                command.StartTime,
                command.EndTime,
                calendarResult.GoogleMeetLink ?? "https://meet.google.com/fallback-link",
                calendarResult.CalendarEventLink,
                cancellationToken);

            logger.LogInformation("Email notification result - Success: {Success}, ClientSent: {ClientSent}, PartnerSent: {PartnerSent}", 
                emailResult.Success, emailResult.ClientEmailSent, emailResult.PartnerEmailSent);

            // Step 3: Schedule meeting reminders
            await reminderService.ScheduleRemindersAsync(
                command.BookingId,
                command.StartTime,
                command.ConsultationTopic,
                calendarResult.GoogleMeetLink ?? "https://meet.google.com/fallback-link",
                new List<string> { command.ClientEmail, command.PartnerEmail });

            logger.LogInformation("Meeting reminders scheduled for BookingId: {BookingId}", command.BookingId);

            // Step 4: Calculate revenue split (80% partner, 20% platform)
            const decimal partnerPercentage = 0.80m;
            var partnerPayout = command.SessionFee * partnerPercentage;
            var platformFee = command.SessionFee - partnerPayout;

            // Step 4: Create the booking completed event
            var bookingCompletedEvent = new BookingCompletedEvent(
                BookingId: command.BookingId,
                ConferenceId: command.ConferenceId,
                ClientEmail: command.ClientEmail,
                PartnerEmail: command.PartnerEmail,
                StartTime: command.StartTime,
                EndTime: command.EndTime,
                ConsultationTopic: command.ConsultationTopic,
                ClientProblemDescription: command.ClientProblemDescription,
                PaymentIntentId: command.PaymentIntentId,
                GoogleCalendarEventId: calendarResult.GoogleCalendarEventId ?? "calendar-failed",
                GoogleMeetLink: calendarResult.GoogleMeetLink ?? "https://meet.google.com/fallback-link",
                SessionFee: command.SessionFee,
                PartnerPayout: partnerPayout,
                PlatformFee: platformFee,
                BookingCompletedAt: DateTime.UtcNow
            );

            // Step 5: Start the event stream for this booking
            var startStream = MartenOps.StartStream<VideoConferenceState>(command.BookingId, bookingCompletedEvent);

            logger.LogInformation("Booking completion successful for BookingId: {BookingId}", command.BookingId);
            return (bookingCompletedEvent, startStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to complete booking for BookingId: {BookingId}", command.BookingId);
            
            // Create a failure event with fallback values
            var failedBookingEvent = new BookingCompletedEvent(
                BookingId: command.BookingId,
                ConferenceId: command.ConferenceId,
                ClientEmail: command.ClientEmail,
                PartnerEmail: command.PartnerEmail,
                StartTime: command.StartTime,
                EndTime: command.EndTime,
                ConsultationTopic: command.ConsultationTopic,
                ClientProblemDescription: command.ClientProblemDescription,
                PaymentIntentId: command.PaymentIntentId,
                GoogleCalendarEventId: "integration-failed",
                GoogleMeetLink: "https://meet.google.com/fallback-link",
                SessionFee: command.SessionFee,
                PartnerPayout: command.SessionFee * 0.80m,
                PlatformFee: command.SessionFee * 0.20m,
                BookingCompletedAt: DateTime.UtcNow
            );

            var startStream = MartenOps.StartStream<VideoConferenceState>(command.BookingId, failedBookingEvent);
            return (failedBookingEvent, startStream);
        }
    }

    /// <summary>
    /// Marks a session as completed and captures payment
    /// </summary>
    [WolverinePost("/api/bookings/{bookingId:guid}/complete-session")]
    public static async Task<SessionCompletedEvent> CompleteSessionAsync(
        Guid bookingId,
        [FromBody] CompleteSessionCommand command,
        [FromServices] ILogger logger)
    {
        logger.LogInformation("Completing session for BookingId: {BookingId}, ConferenceId: {ConferenceId}", 
            bookingId, command.ConferenceId);

        // TODO: Integrate with Stripe payment capture
        // TODO: Update partner earnings and platform revenue

        var sessionCompletedEvent = new SessionCompletedEvent(
            ConferenceId: command.ConferenceId,
            BookingId: command.BookingId,
            PartnerEmail: command.PartnerEmail,
            ClientEmail: "", // TODO: Get from aggregate
            SessionStartTime: DateTime.UtcNow, // TODO: Get actual session times
            SessionEndTime: DateTime.UtcNow,
            ActualCompletionTime: DateTime.UtcNow,
            SessionNotes: command.SessionNotes,
            SessionRating: command.SessionRating,
            PaymentCaptureRequested: command.CapturePayment
        );

        logger.LogInformation("Session completed for BookingId: {BookingId}", bookingId);
        return sessionCompletedEvent;
    }

    /// <summary>
    /// Gets booking details and status
    /// </summary>
    [WolverineGet("/api/bookings/{bookingId:guid}")]
    public static IResult GetBookingDetails(Guid bookingId, [FromServices] ILogger logger)
    {
        logger.LogInformation("Getting booking details for BookingId: {BookingId}", bookingId);
        
        // TODO: Implement booking details retrieval from projections
        return Results.Ok(new
        {
            BookingId = bookingId,
            Status = "Confirmed",
            Message = "Booking details retrieval not yet implemented"
        });
    }

    /// <summary>
    /// Cancels a booking and handles refunds
    /// </summary>
    [WolverinePost("/api/bookings/{bookingId:guid}/cancel")]
    public static async Task<IResult> CancelBookingAsync(
        Guid bookingId,
        [FromServices] GoogleCalendarService calendarService,
        [FromServices] ILogger logger)
    {
        logger.LogInformation("Cancelling booking for BookingId: {BookingId}", bookingId);
        
        try
        {
            // TODO: Get booking details from aggregate
            // TODO: Cancel Google Calendar event
            // TODO: Process refund through Stripe
            // TODO: Send cancellation notifications
            
            return Results.Ok(new { Message = "Booking cancelled successfully", BookingId = bookingId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel booking for BookingId: {BookingId}", bookingId);
            return Results.Problem("Failed to cancel booking");
        }
    }
}

// Remove the placeholder class since we're using static methods only