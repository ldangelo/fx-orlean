using EventServer.Services;
using Fortium.Types;
using Shouldly;
using Xunit.Abstractions;

namespace EventServer.Tests.Services;

public class GoogleCalendarServiceTests
{
    private readonly ITestOutputHelper _output;

    public GoogleCalendarServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConsultationBookingResult_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var result = new ConsultationBookingResult();

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
        result.GoogleCalendarEventId.ShouldBeNull();
        result.GoogleMeetLink.ShouldBeNull();
        result.CalendarEventLink.ShouldBeNull();
        result.PartnerEmail.ShouldBe(string.Empty);
        result.ClientEmail.ShouldBe(string.Empty);
        result.Topic.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConsultationBookingResult_WithSuccessData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        // Act
        var result = new ConsultationBookingResult
        {
            Success = true,
            GoogleCalendarEventId = "event123",
            GoogleMeetLink = "https://meet.google.com/abc-def-ghi",
            CalendarEventLink = "https://calendar.google.com/event?eid=event123",
            StartTime = startTime,
            EndTime = endTime,
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            Topic = "Test Consultation"
        };

        // Assert
        result.Success.ShouldBeTrue();
        result.GoogleCalendarEventId.ShouldBe("event123");
        result.GoogleMeetLink.ShouldBe("https://meet.google.com/abc-def-ghi");
        result.CalendarEventLink.ShouldBe("https://calendar.google.com/event?eid=event123");
        result.StartTime.ShouldBe(startTime);
        result.EndTime.ShouldBe(endTime);
        result.PartnerEmail.ShouldBe("partner@example.com");
        result.ClientEmail.ShouldBe("client@example.com");
        result.Topic.ShouldBe("Test Consultation");
    }

    [Fact]
    public void ConsultationBookingResult_WithErrorData_ShouldHaveErrorInformation()
    {
        // Arrange & Act
        var result = new ConsultationBookingResult
        {
            Success = false,
            ErrorMessage = "Calendar API timeout",
            GoogleMeetLink = "https://meet.google.com/fallback-link", // Fallback provided
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            Topic = "Failed Consultation"
        };

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Calendar API timeout");
        result.GoogleMeetLink.ShouldBe("https://meet.google.com/fallback-link");
        result.PartnerEmail.ShouldBe("partner@example.com");
        result.ClientEmail.ShouldBe("client@example.com");
        result.Topic.ShouldBe("Failed Consultation");
    }

    // Note: GoogleCalendarService integration tests are covered in BookingIntegrationTests
    // The service requires Google API credentials which are better tested through integration tests
}