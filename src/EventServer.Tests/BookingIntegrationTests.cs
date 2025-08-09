using EventServer.Aggregates.VideoConference.Commands;
using EventServer.Aggregates.VideoConference.Events;
using EventServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class BookingIntegrationTests : IntegrationContext
{
    public BookingIntegrationTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task CompleteBooking_WithValidData_ShouldCreateBookingSuccessfully()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new CompleteBookingCommand(
            BookingId: bookingId,
            ConferenceId: conferenceId,
            ClientEmail: "client@example.com",
            PartnerEmail: "leo.dangelo@fortiumpartners.com", // Using test partner
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(1),
            ConsultationTopic: "Cloud Architecture Review",
            ClientProblemDescription: "Need help designing a scalable microservices architecture",
            PaymentIntentId: "pi_test_123456789",
            SessionFee: 800.00m
        );

        // Act & Assert
        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/bookings/complete");
            x.StatusCodeShouldBe(200);
        });

        // Verify response contains booking event
        var response = await result.ReadAsTextAsync();
        response.ShouldContain(bookingId.ToString());
        response.ShouldContain("BookingCompletedAt");
        response.ShouldContain("GoogleMeetLink");
    }

    [Fact]
    public async Task CompleteBooking_WithMissingEmailService_ShouldHandleGracefully()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new CompleteBookingCommand(
            BookingId: bookingId,
            ConferenceId: conferenceId,
            ClientEmail: "invalid-email", // Invalid email to trigger failure
            PartnerEmail: "leo.dangelo@fortiumpartners.com",
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(1),
            ConsultationTopic: "Emergency Architecture Review",
            ClientProblemDescription: "Critical system needs immediate review",
            PaymentIntentId: "pi_test_987654321",
            SessionFee: 800.00m
        );

        // Act & Assert - Should still succeed with fallback values
        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/bookings/complete");
            x.StatusCodeShouldBe(200);
        });

        // Should still create booking event even if email fails
        var response = await result.ReadAsTextAsync();
        response.ShouldContain(bookingId.ToString());
        response.ShouldContain("fallback-link"); // Fallback Google Meet link
    }

    [Fact]
    public async Task CompleteBooking_ShouldCalculateCorrectRevenueSplit()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var sessionFee = 800.00m;
        var expectedPartnerPayout = sessionFee * 0.80m; // 80% = $640
        var expectedPlatformFee = sessionFee * 0.20m; // 20% = $160

        var command = new CompleteBookingCommand(
            BookingId: bookingId,
            ConferenceId: conferenceId,
            ClientEmail: "client@example.com",
            PartnerEmail: "leo.dangelo@fortiumpartners.com",
            StartTime: DateTime.UtcNow.AddDays(2),
            EndTime: DateTime.UtcNow.AddDays(2).AddHours(1),
            ConsultationTopic: "Revenue Split Test",
            ClientProblemDescription: "Testing revenue calculation",
            PaymentIntentId: "pi_revenue_test",
            SessionFee: sessionFee
        );

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/bookings/complete");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var response = await result.ReadAsTextAsync();
        response.ShouldContain($"\"PartnerPayout\":{expectedPartnerPayout}");
        response.ShouldContain($"\"PlatformFee\":{expectedPlatformFee}");
    }

    [Fact]
    public async Task CompleteSession_ShouldMarkSessionAsCompleted()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new CompleteSessionCommand(
            BookingId: bookingId,
            ConferenceId: conferenceId,
            PartnerEmail: "leo.dangelo@fortiumpartners.com",
            SessionNotes: "Great session! Client needs to focus on API gateway architecture and implement circuit breaker patterns.",
            SessionRating: 5,
            CapturePayment: true
        );

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl($"/api/bookings/{bookingId}/complete-session");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var response = await result.ReadAsTextAsync();
        response.ShouldContain("SessionCompletedEvent");
        response.ShouldContain(bookingId.ToString());
        response.ShouldContain("Great session!");
    }

    [Fact]
    public async Task GetBookingDetails_ShouldReturnBookingInformation()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/api/bookings/{bookingId}");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var response = await result.ReadAsTextAsync();
        response.ShouldContain(bookingId.ToString());
        response.ShouldContain("Status");
    }

    [Fact]
    public async Task CancelBooking_ShouldInitiateCancellationProcess()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(new { }).ToUrl($"/api/bookings/{bookingId}/cancel");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var response = await result.ReadAsTextAsync();
        response.ShouldContain("cancelled successfully");
        response.ShouldContain(bookingId.ToString());
    }

    [Fact]
    public async Task CompleteBooking_WithTrackedMessages_ShouldProcessEventCorrectly()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new CompleteBookingCommand(
            BookingId: bookingId,
            ConferenceId: conferenceId,
            ClientEmail: "tracked@example.com",
            PartnerEmail: "burke.autrey@fortiumpartners.com", // Using second test partner
            StartTime: DateTime.UtcNow.AddDays(3),
            EndTime: DateTime.UtcNow.AddDays(3).AddHours(1),
            ConsultationTopic: "Message Tracking Test",
            ClientProblemDescription: "Testing message processing",
            PaymentIntentId: "pi_tracked_test",
            SessionFee: 800.00m
        );

        // Act - Using tracked HTTP call to ensure all message processing completes
        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(command).ToUrl("/api/bookings/complete");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        result.Context.Response.StatusCode.ShouldBe(200);
        
        // Verify that the booking event was processed
        var response = await result.ReadAsTextAsync();
        response.ShouldContain("BookingCompletedEvent");
        response.ShouldContain(bookingId.ToString());
        
        // The tracked session should have detected message activity
        tracked.Sent.MessagesOf<BookingCompletedEvent>().ShouldBeEmpty(); // No outgoing messages expected
    }
}