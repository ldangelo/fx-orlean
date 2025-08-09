using FxExpert.Blazor.Client.Models;
using FxExpert.Blazor.Client.Services;
using Fortium.Types;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FxExpert.Blazor.Client.Tests.Services;

public class CalendarHttpServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<CalendarHttpService>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly CalendarHttpService _calendarHttpService;

    public CalendarHttpServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<CalendarHttpService>>();
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://test.api.com")
        };
        
        _calendarHttpService = new CalendarHttpService(_httpClient, _mockLogger.Object);
    }

    #region GetPartnerAvailabilityNext30DaysAsync Tests

    [Fact]
    public async Task GetPartnerAvailabilityNext30DaysAsync_WhenSuccessful_ReturnsAvailabilityCount()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        const int expectedAvailability = 5;
        
        SetupHttpResponse(HttpStatusCode.OK, expectedAvailability.ToString());

        // Act
        var result = await _calendarHttpService.GetPartnerAvailabilityNext30DaysAsync(partnerEmail);

        // Assert
        result.ShouldBe(expectedAvailability);
        VerifyHttpRequest("POST", "/api/calendar/availability", Times.Once());
    }

    [Fact]
    public async Task GetPartnerAvailabilityNext30DaysAsync_WhenHttpError_ReturnsFallbackValue()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Error");

        // Act
        var result = await _calendarHttpService.GetPartnerAvailabilityNext30DaysAsync(partnerEmail);

        // Assert
        var expectedFallback = Math.Abs(partnerEmail.GetHashCode()) % 8 + 1;
        result.ShouldBe(expectedFallback);
    }

    [Fact]
    public async Task GetPartnerAvailabilityNext30DaysAsync_WhenHttpClientThrows_ReturnsFallbackValue()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _calendarHttpService.GetPartnerAvailabilityNext30DaysAsync(partnerEmail);

        // Assert
        var expectedFallback = Math.Abs(partnerEmail.GetHashCode()) % 8 + 1;
        result.ShouldBe(expectedFallback);
    }

    [Fact]
    public async Task GetPartnerAvailabilityNext30DaysAsync_SendsCorrectRequestPayload()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        HttpRequestMessage? capturedRequest = null;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("5")
            });

        // Act
        await _calendarHttpService.GetPartnerAvailabilityNext30DaysAsync(partnerEmail);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Method.ShouldBe(HttpMethod.Post);
        capturedRequest.RequestUri?.PathAndQuery.ShouldBe("/api/calendar/availability");
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        content.ShouldContain("test@example.com");
        content.ShouldContain("30"); // DaysAhead
    }

    #endregion

    #region IsPartnerAvailableInTimeframeAsync Tests

    [Theory]
    [InlineData(AvailabilityTimeframe.ThisWeek, true)]
    [InlineData(AvailabilityTimeframe.NextWeek, false)]
    [InlineData(AvailabilityTimeframe.ThisMonth, true)]
    public async Task IsPartnerAvailableInTimeframeAsync_WhenSuccessful_ReturnsCorrectValue(
        AvailabilityTimeframe timeframe, bool expectedResult)
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        
        SetupHttpResponse(HttpStatusCode.OK, expectedResult.ToString().ToLower());

        // Act
        var result = await _calendarHttpService.IsPartnerAvailableInTimeframeAsync(partnerEmail, timeframe);

        // Assert
        result.ShouldBe(expectedResult);
        VerifyHttpRequest("POST", "/api/calendar/availability/timeframe", Times.Once());
    }

    [Fact]
    public async Task IsPartnerAvailableInTimeframeAsync_WhenHttpError_ReturnsTrue()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        
        SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

        // Act
        var result = await _calendarHttpService.IsPartnerAvailableInTimeframeAsync(
            partnerEmail, AvailabilityTimeframe.ThisWeek);

        // Assert
        result.ShouldBeTrue(); // Should assume available on error to avoid false negatives
    }

    [Fact]
    public async Task IsPartnerAvailableInTimeframeAsync_SendsCorrectTimeframe()
    {
        // Arrange
        const string partnerEmail = "test@example.com";
        const AvailabilityTimeframe timeframe = AvailabilityTimeframe.NextWeek;
        HttpRequestMessage? capturedRequest = null;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("true")
            });

        // Act
        await _calendarHttpService.IsPartnerAvailableInTimeframeAsync(partnerEmail, timeframe);

        // Assert
        capturedRequest.ShouldNotBeNull();
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        content.ShouldContain("NextWeek");
        content.ShouldContain(partnerEmail);
    }

    #endregion

    #region RefreshMultiplePartnerAvailabilityAsync Tests

    [Fact]
    public async Task RefreshMultiplePartnerAvailabilityAsync_WhenSuccessful_ReturnsAvailabilityDictionary()
    {
        // Arrange
        var partnerEmails = new List<string> { "partner1@example.com", "partner2@example.com" };
        var expectedResult = new Dictionary<string, int>
        {
            { "partner1@example.com", 5 },
            { "partner2@example.com", 3 }
        };
        
        var jsonResponse = JsonSerializer.Serialize(expectedResult);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _calendarHttpService.RefreshMultiplePartnerAvailabilityAsync(partnerEmails);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result["partner1@example.com"].ShouldBe(5);
        result["partner2@example.com"].ShouldBe(3);
        VerifyHttpRequest("POST", "/api/calendar/availability/batch", Times.Once());
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailabilityAsync_WithEmptyList_ReturnsEmptyDictionary()
    {
        // Arrange
        var partnerEmails = new List<string>();

        // Act
        var result = await _calendarHttpService.RefreshMultiplePartnerAvailabilityAsync(partnerEmails);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailabilityAsync_WhenHttpError_ReturnsFallbackValues()
    {
        // Arrange
        var partnerEmails = new List<string> { "partner1@example.com", "partner2@example.com" };
        
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        // Act
        var result = await _calendarHttpService.RefreshMultiplePartnerAvailabilityAsync(partnerEmails);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        
        // Verify fallback values are based on email hash
        result["partner1@example.com"].ShouldBe(Math.Abs("partner1@example.com".GetHashCode()) % 8 + 1);
        result["partner2@example.com"].ShouldBe(Math.Abs("partner2@example.com".GetHashCode()) % 8 + 1);
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailabilityAsync_WhenExceptionThrown_ReturnsFallbackValues()
    {
        // Arrange
        var partnerEmails = new List<string> { "test@example.com" };
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result = await _calendarHttpService.RefreshMultiplePartnerAvailabilityAsync(partnerEmails);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result["test@example.com"].ShouldBe(Math.Abs("test@example.com".GetHashCode()) % 8 + 1);
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailabilityAsync_SendsCorrectPayload()
    {
        // Arrange
        var partnerEmails = new List<string> { "partner1@example.com", "partner2@example.com" };
        HttpRequestMessage? capturedRequest = null;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"partner1@example.com\":5,\"partner2@example.com\":3}")
            });

        // Act
        await _calendarHttpService.RefreshMultiplePartnerAvailabilityAsync(partnerEmails);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Method.ShouldBe(HttpMethod.Post);
        capturedRequest.RequestUri?.PathAndQuery.ShouldBe("/api/calendar/availability/batch");
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        content.ShouldContain("partner1@example.com");
        content.ShouldContain("partner2@example.com");
        content.ShouldContain("PartnerEmails");
    }

    #endregion

    #region CreateConsultationBookingAsync Tests

    [Fact]
    public async Task CreateConsultationBookingAsync_WhenSuccessful_ReturnsBookingResponse()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Topic = "Strategy Discussion",
            ProblemDescription = "Need help with digital transformation"
        };

        var expectedResponse = new ConsultationBookingResponse
        {
            Success = true,
            GoogleCalendarEventId = "event123",
            GoogleMeetLink = "https://meet.google.com/abc-def-ghi",
            CalendarEventLink = "https://calendar.google.com/event123",
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            PartnerEmail = request.PartnerEmail,
            ClientEmail = request.ClientEmail,
            Topic = request.Topic
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _calendarHttpService.CreateConsultationBookingAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        result.GoogleCalendarEventId.ShouldBe("event123");
        result.GoogleMeetLink.ShouldBe("https://meet.google.com/abc-def-ghi");
        result.PartnerEmail.ShouldBe(request.PartnerEmail);
        result.ClientEmail.ShouldBe(request.ClientEmail);
        VerifyHttpRequest("POST", "/api/calendar/booking", Times.Once());
    }

    [Fact]
    public async Task CreateConsultationBookingAsync_WhenHttpError_ReturnsFailureResponse()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Topic = "Strategy Discussion",
            ProblemDescription = "Need help with digital transformation"
        };

        SetupHttpResponse(HttpStatusCode.BadRequest, "Validation Error");

        // Act
        var result = await _calendarHttpService.CreateConsultationBookingAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("BadRequest");
        result.GoogleMeetLink.ShouldNotBeNull(); // Should provide fallback meet link
        result.GoogleMeetLink.ShouldStartWith("https://meet.google.com/");
        result.StartTime.ShouldBe(request.StartTime);
        result.EndTime.ShouldBe(request.EndTime);
        result.PartnerEmail.ShouldBe(request.PartnerEmail);
        result.ClientEmail.ShouldBe(request.ClientEmail);
    }

    [Fact]
    public async Task CreateConsultationBookingAsync_WhenExceptionThrown_ReturnsFailureResponse()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Topic = "Strategy Discussion",
            ProblemDescription = "Need help"
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        // Act
        var result = await _calendarHttpService.CreateConsultationBookingAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("Connection timeout");
        result.GoogleMeetLink.ShouldNotBeNull(); // Should provide fallback meet link
    }

    [Fact]
    public async Task CreateConsultationBookingAsync_SendsCorrectRequestData()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Topic = "Strategy Discussion",
            ProblemDescription = "Need help with digital transformation"
        };

        HttpRequestMessage? capturedRequest = null;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Success\":true,\"GoogleMeetLink\":\"https://meet.google.com/test\"}")
            });

        // Act
        await _calendarHttpService.CreateConsultationBookingAsync(request);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Method.ShouldBe(HttpMethod.Post);
        capturedRequest.RequestUri?.PathAndQuery.ShouldBe("/api/calendar/booking");
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        content.ShouldContain("partner@example.com");
        content.ShouldContain("client@example.com");
        content.ShouldContain("Strategy Discussion");
        content.ShouldContain("digital transformation");
    }

    #endregion

    #region Fallback Meet Link Generation Tests

    [Fact]
    public void GenerateFallbackMeetLink_CreatesValidGoogleMeetUrl()
    {
        // We can't directly test the private method, but we can verify it indirectly
        // by checking the fallback behavior in error scenarios
        
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "partner@example.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Topic = "Test",
            ProblemDescription = "Test"
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test error"));

        // Act
        var result = _calendarHttpService.CreateConsultationBookingAsync(request).Result;

        // Assert
        result.GoogleMeetLink.ShouldNotBeNull();
        result.GoogleMeetLink.ShouldStartWith("https://meet.google.com/");
        result.GoogleMeetLink.ShouldMatch(@"https://meet\.google\.com/[\w]+-[\w]+-[\w]+");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetPartnerAvailabilityNext30DaysAsync_RespectsCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("5")
            });

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _calendarHttpService.GetPartnerAvailabilityNext30DaysAsync("test@example.com", cts.Token));
    }

    [Fact]
    public async Task IsPartnerAvailableInTimeframeAsync_RespectsCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("true")
            });

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _calendarHttpService.IsPartnerAvailableInTimeframeAsync("test@example.com", AvailabilityTimeframe.ThisWeek, cts.Token));
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(string method, string expectedPath, Times times)
    {
        _mockHttpMessageHandler.Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method.ToString().ToUpper() == method.ToUpper() &&
                    req.RequestUri != null &&
                    req.RequestUri.PathAndQuery == expectedPath),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}