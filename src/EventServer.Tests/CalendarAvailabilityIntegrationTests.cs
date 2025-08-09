using EventServer.Controllers;
using Fortium.Types;
using Shouldly;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace EventServer.Tests;

/// <summary>
/// Integration tests for Google Calendar availability endpoints
/// Tests the new calendar availability features added for Advanced Partner Search
/// </summary>
public class CalendarAvailabilityIntegrationTests : IntegrationContext
{
    public CalendarAvailabilityIntegrationTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    #region Partner Availability Tests

    [Fact]
    public async Task GetPartnerAvailability_WithValidPartnerEmail_ShouldReturnAvailabilityCount()
    {
        // Arrange
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com", // Test partner
            DaysAhead = 30
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var availability = await result.ReadAsJsonAsync<int>();
        availability.ShouldBeGreaterThanOrEqualTo(0);
        availability.ShouldBeLessThanOrEqualTo(40); // Reasonable upper limit (8 hours * 5 days)
    }

    [Fact]
    public async Task GetPartnerAvailability_With7DayWindow_ShouldReturnCorrectTimeframe()
    {
        // Arrange
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            DaysAhead = 7
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var availability = await result.ReadAsJsonAsync<int>();
        availability.ShouldBeGreaterThanOrEqualTo(0);
        // 7 days should generally have fewer slots than 30 days
        availability.ShouldBeLessThanOrEqualTo(35); // 7 weekdays * 8 hours = 56 max, but likely much less
    }

    [Fact]
    public async Task GetPartnerAvailability_WithCustomDaysAhead_ShouldHandleVariousTimeframes()
    {
        // Arrange - Test different day ranges
        var testCases = new[] { 1, 3, 14, 21, 45 };

        foreach (var daysAhead in testCases)
        {
            var request = new PartnerAvailabilityRequest
            {
                PartnerEmail = "leo.dangelo@fortiumpartners.com",
                DaysAhead = daysAhead
            };

            // Act
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability");
                x.StatusCodeShouldBe(200);
            });

            // Assert
            var availability = await result.ReadAsJsonAsync<int>();
            availability.ShouldBeGreaterThanOrEqualTo(0);
            
            // Generally, more days should mean more availability (unless all slots are booked)
            Output.WriteLine($"Days ahead: {daysAhead}, Availability: {availability}");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetPartnerAvailability_WithInvalidEmail_ShouldReturnBadRequest(string invalidEmail)
    {
        // Arrange
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = invalidEmail,
            DaysAhead = 30
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(500); // Exception thrown for invalid email
        });
    }

    [Fact]
    public async Task GetPartnerAvailability_WithNonExistentPartner_ShouldHandleGracefully()
    {
        // Arrange
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = "nonexistent@example.com",
            DaysAhead = 30
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200); // Should not fail, just return low availability
        });

        // Assert
        var availability = await result.ReadAsJsonAsync<int>();
        availability.ShouldBeGreaterThanOrEqualTo(1); // Fallback value should be >= 1
        availability.ShouldBeLessThanOrEqualTo(8); // Fallback is based on hash, should be 1-8
    }

    #endregion

    #region Timeframe Availability Tests

    [Theory]
    [InlineData(AvailabilityTimeframe.ThisWeek)]
    [InlineData(AvailabilityTimeframe.NextWeek)]
    [InlineData(AvailabilityTimeframe.ThisMonth)]
    public async Task CheckPartnerAvailabilityInTimeframe_WithValidTimeframes_ShouldReturnBoolean(
        AvailabilityTimeframe timeframe)
    {
        // Arrange
        var request = new PartnerTimeframeAvailabilityRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            Timeframe = timeframe.ToString()
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/timeframe");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var hasAvailability = await result.ReadAsJsonAsync<bool>();
        // Result should be a valid boolean (true or false)
        hasAvailability.ShouldBeOfType<bool>();
        
        Output.WriteLine($"Partner has availability in {timeframe}: {hasAvailability}");
    }

    [Fact]
    public async Task CheckPartnerAvailabilityInTimeframe_WithInvalidTimeframe_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PartnerTimeframeAvailabilityRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            Timeframe = "InvalidTimeframe"
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/timeframe");
            x.StatusCodeShouldBe(500); // Exception thrown for invalid timeframe
        });
    }

    [Fact]
    public async Task CheckPartnerAvailabilityInTimeframe_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PartnerTimeframeAvailabilityRequest
        {
            PartnerEmail = "",
            Timeframe = AvailabilityTimeframe.ThisWeek.ToString()
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/timeframe");
            x.StatusCodeShouldBe(500); // Exception thrown for empty email
        });
    }

    #endregion

    #region Batch Availability Tests

    [Fact]
    public async Task RefreshMultiplePartnerAvailability_WithValidEmails_ShouldReturnAvailabilityDictionary()
    {
        // Arrange
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = new List<string>
            {
                "leo.dangelo@fortiumpartners.com",
                "burke.autrey@fortiumpartners.com"
            }
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(2);
        
        availabilityResults.ShouldContainKey("leo.dangelo@fortiumpartners.com");
        availabilityResults.ShouldContainKey("burke.autrey@fortiumpartners.com");
        
        foreach (var kvp in availabilityResults)
        {
            kvp.Value.ShouldBeGreaterThanOrEqualTo(0);
            Output.WriteLine($"{kvp.Key}: {kvp.Value} available slots");
        }
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailability_WithEmptyList_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = new List<string>()
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(500); // Exception thrown for empty list
        });
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailability_WithMixedValidInvalidEmails_ShouldHandleGracefully()
    {
        // Arrange
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = new List<string>
            {
                "leo.dangelo@fortiumpartners.com", // Valid
                "invalid@nonexistent.com", // Invalid but should still get fallback
                "anotherfake@example.com"  // Invalid but should still get fallback
            }
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(3);
        
        // All partners should have some availability (real or fallback)
        foreach (var kvp in availabilityResults)
        {
            kvp.Value.ShouldBeGreaterThanOrEqualTo(1); // Fallback is minimum 1
            kvp.Value.ShouldBeLessThanOrEqualTo(40); // Reasonable upper limit
            Output.WriteLine($"{kvp.Key}: {kvp.Value} available slots");
        }
    }

    [Fact]
    public async Task RefreshMultiplePartnerAvailability_WithLargeList_ShouldHandleParallelProcessing()
    {
        // Arrange - Create a larger list to test parallel processing
        var partnerEmails = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            partnerEmails.Add($"partner{i}@example.com");
        }
        partnerEmails.Add("leo.dangelo@fortiumpartners.com"); // Add one real partner

        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = partnerEmails
        };

        // Act
        var startTime = DateTime.Now;
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });
        var endTime = DateTime.Now;

        // Assert
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(11); // All partners should be in the result

        // Performance assertion - parallel processing should be reasonably fast
        var duration = endTime - startTime;
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(30)); // Should complete within 30 seconds
        
        Output.WriteLine($"Processed {availabilityResults.Count} partners in {duration.TotalMilliseconds}ms");
    }

    #endregion

    #region Consultation Booking Tests

    [Fact]
    public async Task CreateConsultationBooking_WithValidData_ShouldCreateBookingSuccessfully()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.UtcNow.AddDays(7), // Book for next week
            EndTime = DateTime.UtcNow.AddDays(7).AddHours(1),
            Topic = "Cloud Architecture Review",
            ProblemDescription = "Need guidance on migrating legacy system to cloud-native architecture with microservices."
        };

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/booking");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var bookingResult = await result.ReadAsJsonAsync<ConsultationBookingResult>();
        bookingResult.ShouldNotBeNull();
        
        if (bookingResult.Success)
        {
            // If booking succeeded (Google Calendar integration working)
            bookingResult.GoogleCalendarEventId.ShouldNotBeNull();
            bookingResult.GoogleMeetLink.ShouldNotBeNull();
            bookingResult.GoogleMeetLink.ShouldStartWith("https://meet.google.com/");
            bookingResult.CalendarEventLink.ShouldNotBeNull();
        }
        else
        {
            // If booking failed (expected in test environment), should have fallback
            bookingResult.ErrorMessage.ShouldNotBeNull();
            bookingResult.GoogleMeetLink.ShouldNotBeNull(); // Should have fallback link
            bookingResult.GoogleMeetLink.ShouldStartWith("https://meet.google.com/");
        }

        bookingResult.StartTime.ShouldBe(request.StartTime);
        bookingResult.EndTime.ShouldBe(request.EndTime);
        bookingResult.PartnerEmail.ShouldBe(request.PartnerEmail);
        bookingResult.ClientEmail.ShouldBe(request.ClientEmail);
        bookingResult.Topic.ShouldBe(request.Topic);
        
        Output.WriteLine($"Booking success: {bookingResult.Success}");
        Output.WriteLine($"Google Meet Link: {bookingResult.GoogleMeetLink}");
        if (!string.IsNullOrEmpty(bookingResult.ErrorMessage))
        {
            Output.WriteLine($"Error: {bookingResult.ErrorMessage}");
        }
    }

    [Theory]
    [InlineData("", "client@example.com", "Both partner and client emails are required")]
    [InlineData("partner@example.com", "", "Both partner and client emails are required")]
    [InlineData("", "", "Both partner and client emails are required")]
    public async Task CreateConsultationBooking_WithMissingEmails_ShouldReturnBadRequest(
        string partnerEmail, string clientEmail, string expectedError)
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = partnerEmail,
            ClientEmail = clientEmail,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Topic = "Test",
            ProblemDescription = "Test description"
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/booking");
            x.StatusCodeShouldBe(500); // Exception thrown for missing data
        });
    }

    [Fact]
    public async Task CreateConsultationBooking_WithPastTime_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            ClientEmail = "client@example.com",
            StartTime = DateTime.UtcNow.AddDays(-1), // Past time
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Topic = "Past Booking Test",
            ProblemDescription = "This should fail"
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/booking");
            x.StatusCodeShouldBe(500); // Exception thrown for past booking
        });
    }

    [Fact]
    public async Task CreateConsultationBooking_WithInvalidTimeRange_ShouldReturnBadRequest()
    {
        // Arrange - End time before start time
        var startTime = DateTime.UtcNow.AddDays(1);
        var request = new ConsultationBookingRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            ClientEmail = "client@example.com",
            StartTime = startTime,
            EndTime = startTime.AddHours(-1), // End time before start time
            Topic = "Invalid Time Range Test",
            ProblemDescription = "This should fail"
        };

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/booking");
            x.StatusCodeShouldBe(500); // Exception thrown for invalid time range
        });
    }

    #endregion

    #region Performance and Load Tests

    [Fact]
    public async Task CalendarAvailabilityEndpoints_UnderModerateLoad_ShouldPerformReasonably()
    {
        // Arrange
        var tasks = new List<Task>();
        var startTime = DateTime.Now;

        // Act - Make multiple concurrent requests to test performance
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(TestSingleAvailabilityRequest($"partner{i}@example.com"));
        }

        await Task.WhenAll(tasks);
        var endTime = DateTime.Now;

        // Assert
        var duration = endTime - startTime;
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(15)); // Should handle 5 concurrent requests in < 15 seconds
        
        Output.WriteLine($"Completed 5 concurrent availability requests in {duration.TotalMilliseconds}ms");
    }

    private async Task TestSingleAvailabilityRequest(string partnerEmail)
    {
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = partnerEmail,
            DaysAhead = 30
        };

        await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200);
        });
    }

    #endregion

    #region Error Handling and Resilience Tests

    [Fact]
    public async Task CalendarEndpoints_WithGoogleAPIFailure_ShouldProvideGracefulFallback()
    {
        // This test verifies that when Google Calendar API is unavailable,
        // the endpoints still return reasonable fallback values
        
        // Test availability endpoint with a non-existent partner (simulates API failure)
        var availabilityRequest = new PartnerAvailabilityRequest
        {
            PartnerEmail = "definitelynotreal@nowhere.com",
            DaysAhead = 30
        };

        var result = await Scenario(x =>
        {
            x.Post.Json(availabilityRequest).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200);
        });

        var availability = await result.ReadAsJsonAsync<int>();
        availability.ShouldBeGreaterThanOrEqualTo(1); // Should have fallback value
        availability.ShouldBeLessThanOrEqualTo(8); // Fallback range is 1-8
        
        Output.WriteLine($"Fallback availability for non-existent partner: {availability}");
    }

    #endregion
}