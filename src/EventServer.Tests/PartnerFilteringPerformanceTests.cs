using EventServer.Controllers;
using Fortium.Types;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Diagnostics;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace EventServer.Tests;

/// <summary>
/// Performance tests for partner filtering with large datasets
/// Tests calendar integration performance under various load conditions
/// </summary>
public class PartnerFilteringPerformanceTests : IntegrationContext
{
    public PartnerFilteringPerformanceTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    #region Single Partner Availability Performance Tests

    [Fact]
    public async Task PartnerAvailability_SingleRequest_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var request = new PartnerAvailabilityRequest
        {
            PartnerEmail = "leo.dangelo@fortiumpartners.com",
            DaysAhead = 30
        };
        
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability");
            x.StatusCodeShouldBe(200);
        });
        stopwatch.Stop();

        // Assert
        var availability = await result.ReadAsJsonAsync<int>();
        availability.ShouldBeGreaterThanOrEqualTo(0);
        
        // Performance assertion - should complete within 5 seconds
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000, 
            $"Single availability request took {stopwatch.ElapsedMilliseconds}ms, should be < 5000ms");
        
        Output.WriteLine($"Single availability request completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task PartnerAvailability_MultipleTimeframes_ShouldMaintainPerformance()
    {
        // Arrange
        var timeframes = new[] { 7, 14, 30, 45 };
        var partnerEmail = "leo.dangelo@fortiumpartners.com";
        var results = new List<(int Days, long ElapsedMs, int Availability)>();

        // Act
        foreach (var days in timeframes)
        {
            var request = new PartnerAvailabilityRequest
            {
                PartnerEmail = partnerEmail,
                DaysAhead = days
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability");
                x.StatusCodeShouldBe(200);
            });
            stopwatch.Stop();
            
            var availability = await result.ReadAsJsonAsync<int>();
            results.Add((days, stopwatch.ElapsedMilliseconds, availability));
        }

        // Assert
        foreach (var (days, elapsed, availability) in results)
        {
            elapsed.ShouldBeLessThan(6000, $"Availability request for {days} days took {elapsed}ms, should be < 6000ms");
            availability.ShouldBeGreaterThanOrEqualTo(0);
            
            Output.WriteLine($"Availability for {days} days: {availability} slots in {elapsed}ms");
        }
        
        // Performance should not degrade significantly with longer timeframes
        var avgElapsedMs = results.Average(r => r.ElapsedMs);
        avgElapsedMs.ShouldBeLessThan(4000, $"Average response time was {avgElapsedMs}ms, should be < 4000ms");
    }

    #endregion

    #region Batch Availability Performance Tests

    [Fact]
    public async Task BatchAvailability_SmallBatch_ShouldOutperformIndividualRequests()
    {
        // Arrange
        var partnerEmails = new List<string>
        {
            "leo.dangelo@fortiumpartners.com",
            "burke.autrey@fortiumpartners.com",
            "partner1@example.com",
            "partner2@example.com",
            "partner3@example.com"
        };
        
        // Test individual requests
        var individualStopwatch = Stopwatch.StartNew();
        var individualResults = new Dictionary<string, int>();
        
        foreach (var email in partnerEmails)
        {
            var request = new PartnerAvailabilityRequest
            {
                PartnerEmail = email,
                DaysAhead = 30
            };
            
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability");
                x.StatusCodeShouldBe(200);
            });
            
            var availability = await result.ReadAsJsonAsync<int>();
            individualResults[email] = availability;
        }
        individualStopwatch.Stop();
        
        // Test batch request
        var batchRequest = new BatchAvailabilityRequest
        {
            PartnerEmails = partnerEmails
        };
        
        var batchStopwatch = Stopwatch.StartNew();
        var batchResult = await Scenario(x =>
        {
            x.Post.Json(batchRequest).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });
        batchStopwatch.Stop();

        // Assert
        var batchResults = await batchResult.ReadAsJsonAsync<Dictionary<string, int>>();
        batchResults.ShouldNotBeNull();
        batchResults.Count.ShouldBe(partnerEmails.Count);
        
        // Batch should be significantly faster than individual requests
        var performanceImprovement = (double)individualStopwatch.ElapsedMilliseconds / batchStopwatch.ElapsedMilliseconds;
        performanceImprovement.ShouldBeGreaterThan(1.5, 
            $"Batch processing should be at least 50% faster. Individual: {individualStopwatch.ElapsedMilliseconds}ms, Batch: {batchStopwatch.ElapsedMilliseconds}ms");
        
        Output.WriteLine($"Individual requests: {individualStopwatch.ElapsedMilliseconds}ms");
        Output.WriteLine($"Batch request: {batchStopwatch.ElapsedMilliseconds}ms");
        Output.WriteLine($"Performance improvement: {performanceImprovement:F2}x faster");
    }

    [Fact]
    public async Task BatchAvailability_MediumBatch_ShouldHandleReasonableLoad()
    {
        // Arrange - Create medium batch (20 partners)
        var partnerEmails = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            partnerEmails.Add($"partner{i:D2}@example.com");
        }
        partnerEmails.Add("leo.dangelo@fortiumpartners.com"); // Add one real partner
        
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = partnerEmails
        };
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });
        stopwatch.Stop();

        // Assert
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(21); // All partners should be processed
        
        // Performance assertion - should handle 21 partners within reasonable time
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(25000, 
            $"Medium batch (21 partners) took {stopwatch.ElapsedMilliseconds}ms, should be < 25000ms");
        
        // Verify all partners have availability values
        foreach (var kvp in availabilityResults)
        {
            kvp.Value.ShouldBeGreaterThanOrEqualTo(1, $"Partner {kvp.Key} should have at least fallback availability");
        }
        
        Output.WriteLine($"Medium batch (21 partners) completed in {stopwatch.ElapsedMilliseconds}ms");
        Output.WriteLine($"Average time per partner: {stopwatch.ElapsedMilliseconds / 21.0:F1}ms");
    }

    [Fact]
    public async Task BatchAvailability_LargeBatch_ShouldScaleReasonably()
    {
        // Arrange - Create large batch (50 partners)
        var partnerEmails = new List<string>();
        for (int i = 1; i <= 50; i++)
        {
            partnerEmails.Add($"partner{i:D3}@example.com");
        }
        partnerEmails.Add("leo.dangelo@fortiumpartners.com"); // Add one real partner
        
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = partnerEmails
        };
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });
        stopwatch.Stop();

        // Assert
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(51); // All partners should be processed
        
        // Performance assertion - should handle 51 partners within reasonable time
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(60000, 
            $"Large batch (51 partners) took {stopwatch.ElapsedMilliseconds}ms, should be < 60000ms");
        
        // Calculate performance metrics
        var avgTimePerPartner = stopwatch.ElapsedMilliseconds / 51.0;
        avgTimePerPartner.ShouldBeLessThan(1200, $"Average time per partner was {avgTimePerPartner:F1}ms, should be < 1200ms");
        
        Output.WriteLine($"Large batch (51 partners) completed in {stopwatch.ElapsedMilliseconds}ms");
        Output.WriteLine($"Average time per partner: {avgTimePerPartner:F1}ms");
        
        // Verify reasonable distribution of availability values
        var availabilityValues = availabilityResults.Values.ToList();
        var minAvailability = availabilityValues.Min();
        var maxAvailability = availabilityValues.Max();
        var avgAvailability = availabilityValues.Average();
        
        Output.WriteLine($"Availability distribution: Min={minAvailability}, Max={maxAvailability}, Avg={avgAvailability:F1}");
        
        minAvailability.ShouldBeGreaterThanOrEqualTo(1);
        maxAvailability.ShouldBeLessThanOrEqualTo(10); // Reasonable upper bound for fallback
    }

    #endregion

    #region Concurrent Request Performance Tests

    [Fact]
    public async Task AvailabilityEndpoints_ConcurrentRequests_ShouldMaintainPerformance()
    {
        // Arrange - Prepare multiple concurrent requests
        var concurrentTasks = new List<Task<(string Type, long ElapsedMs, bool Success)>>();
        
        // Individual availability requests
        for (int i = 1; i <= 5; i++)
        {
            concurrentTasks.Add(TestSingleAvailabilityRequestAsync($"partner{i}@example.com"));
        }
        
        // Batch availability requests
        for (int i = 1; i <= 3; i++)
        {
            var batchEmails = new List<string> 
            { 
                $"batch{i}a@example.com", 
                $"batch{i}b@example.com", 
                $"batch{i}c@example.com" 
            };
            concurrentTasks.Add(TestBatchAvailabilityRequestAsync(batchEmails, $"Batch{i}"));
        }
        
        // Timeframe availability requests
        var timeframeTests = new[]
        {
            ("ThisWeek", AvailabilityTimeframe.ThisWeek),
            ("NextWeek", AvailabilityTimeframe.NextWeek),
            ("ThisMonth", AvailabilityTimeframe.ThisMonth)
        };
        
        foreach (var (name, timeframe) in timeframeTests)
        {
            concurrentTasks.Add(TestTimeframeAvailabilityRequestAsync($"timeframe{name}@example.com", timeframe, name));
        }
        
        var overallStopwatch = Stopwatch.StartNew();

        // Act - Execute all requests concurrently
        var results = await Task.WhenAll(concurrentTasks);
        overallStopwatch.Stop();

        // Assert
        var successfulRequests = results.Where(r => r.Success).ToList();
        var failedRequests = results.Where(r => !r.Success).ToList();
        
        // Most requests should succeed
        successfulRequests.Count.ShouldBeGreaterThan(results.Length * 0.8, 
            $"At least 80% of concurrent requests should succeed. Success: {successfulRequests.Count}, Failed: {failedRequests.Count}");
        
        // Overall time should be reasonable (not much longer than the longest individual request)
        var maxIndividualTime = results.Max(r => r.ElapsedMs);
        overallStopwatch.ElapsedMilliseconds.ShouldBeLessThan(maxIndividualTime + 5000, 
            $"Concurrent execution took {overallStopwatch.ElapsedMilliseconds}ms, max individual was {maxIndividualTime}ms");
        
        // Performance statistics
        var avgTime = successfulRequests.Average(r => r.ElapsedMs);
        avgTime.ShouldBeLessThan(8000, $"Average response time was {avgTime:F1}ms, should be < 8000ms");
        
        Output.WriteLine($"Concurrent test results:");
        Output.WriteLine($"Total requests: {results.Length}");
        Output.WriteLine($"Successful: {successfulRequests.Count}");
        Output.WriteLine($"Failed: {failedRequests.Count}");
        Output.WriteLine($"Overall time: {overallStopwatch.ElapsedMilliseconds}ms");
        Output.WriteLine($"Average response time: {avgTime:F1}ms");
        Output.WriteLine($"Max individual time: {maxIndividualTime}ms");
        
        foreach (var result in results)
        {
            Output.WriteLine($"  {result.Type}: {result.ElapsedMs}ms {(result.Success ? "✓" : "✗")}");
        }
    }

    #endregion

    #region Memory and Resource Usage Tests

    [Fact]
    public async Task BatchAvailability_LargeDataset_ShouldNotExceedMemoryLimits()
    {
        // Arrange - Create very large batch (100 partners)
        var partnerEmails = new List<string>();
        for (int i = 1; i <= 100; i++)
        {
            partnerEmails.Add($"partner{i:D3}@example.com");
        }
        
        var request = new BatchAvailabilityRequest
        {
            PartnerEmails = partnerEmails
        };
        
        // Get initial memory usage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
            x.StatusCodeShouldBe(200);
        });
        stopwatch.Stop();
        
        var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
        
        // Get final memory usage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        availabilityResults.ShouldNotBeNull();
        availabilityResults.Count.ShouldBe(100);
        
        // Performance assertions
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(120000, // 2 minutes
            $"Very large batch (100 partners) took {stopwatch.ElapsedMilliseconds}ms, should be < 120000ms");
        
        // Memory usage should not increase dramatically
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseKB = memoryIncrease / 1024.0;
        memoryIncreaseKB.ShouldBeLessThan(50000, // 50MB increase limit
            $"Memory increased by {memoryIncreaseKB:F1}KB, should be < 50000KB");
        
        Output.WriteLine($"Very large batch (100 partners) performance:");
        Output.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedMilliseconds/100.0:F1}ms per partner)");
        Output.WriteLine($"Memory change: {memoryIncreaseKB:F1}KB");
        Output.WriteLine($"Initial memory: {initialMemory / 1024.0:F1}KB");
        Output.WriteLine($"Final memory: {finalMemory / 1024.0:F1}KB");
    }

    #endregion

    #region Calendar Service Stress Tests

    [Fact]
    public async Task CalendarService_RepeatedRequests_ShouldHandleConsistently()
    {
        // Arrange - Test repeated requests to the same partner
        const int requestCount = 10;
        const string partnerEmail = "leo.dangelo@fortiumpartners.com";
        var results = new List<(int Availability, long ElapsedMs)>();
        
        // Act - Make repeated requests
        for (int i = 0; i < requestCount; i++)
        {
            var request = new PartnerAvailabilityRequest
            {
                PartnerEmail = partnerEmail,
                DaysAhead = 30
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability");
                x.StatusCodeShouldBe(200);
            });
            stopwatch.Stop();
            
            var availability = await result.ReadAsJsonAsync<int>();
            results.Add((availability, stopwatch.ElapsedMilliseconds));
            
            // Small delay between requests to avoid rate limiting
            await Task.Delay(100);
        }

        // Assert
        results.Count.ShouldBe(requestCount);
        
        // All requests should succeed
        foreach (var result in results)
        {
            result.Availability.ShouldBeGreaterThanOrEqualTo(0);
            result.ElapsedMs.ShouldBeLessThan(10000, $"Individual request took {result.ElapsedMs}ms, should be < 10000ms");
        }
        
        // Performance should be consistent (no degradation)
        var avgTime = results.Average(r => r.ElapsedMs);
        var maxTime = results.Max(r => r.ElapsedMs);
        var minTime = results.Min(r => r.ElapsedMs);
        
        var performanceVariance = (maxTime - minTime) / (double)avgTime;
        performanceVariance.ShouldBeLessThan(2.0, 
            $"Performance variance was {performanceVariance:F2}, should be < 2.0 (consistent performance)");
        
        // Results should be consistent (same partner, same timeframe should give same result)
        var availabilityValues = results.Select(r => r.Availability).Distinct().ToList();
        availabilityValues.Count.ShouldBeLessThanOrEqualTo(2, 
            "Availability for same partner should be consistent (allowing for 1 change during test period)");
        
        Output.WriteLine($"Repeated requests performance:");
        Output.WriteLine($"Requests: {requestCount}");
        Output.WriteLine($"Average time: {avgTime:F1}ms");
        Output.WriteLine($"Min time: {minTime}ms");
        Output.WriteLine($"Max time: {maxTime}ms");
        Output.WriteLine($"Performance variance: {performanceVariance:F2}");
        Output.WriteLine($"Unique availability values: {string.Join(", ", availabilityValues)}");
    }

    #endregion

    #region Helper Methods

    private async Task<(string Type, long ElapsedMs, bool Success)> TestSingleAvailabilityRequestAsync(string partnerEmail)
    {
        try
        {
            var request = new PartnerAvailabilityRequest
            {
                PartnerEmail = partnerEmail,
                DaysAhead = 30
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability");
                x.StatusCodeShouldBe(200);
            });
            stopwatch.Stop();
            
            var availability = await result.ReadAsJsonAsync<int>();
            return ($"Single-{partnerEmail}", stopwatch.ElapsedMilliseconds, availability >= 0);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Single availability request failed for {partnerEmail}: {ex.Message}");
            return ($"Single-{partnerEmail}", 0, false);
        }
    }

    private async Task<(string Type, long ElapsedMs, bool Success)> TestBatchAvailabilityRequestAsync(List<string> partnerEmails, string testName)
    {
        try
        {
            var request = new BatchAvailabilityRequest
            {
                PartnerEmails = partnerEmails
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability/batch");
                x.StatusCodeShouldBe(200);
            });
            stopwatch.Stop();
            
            var availabilityResults = await result.ReadAsJsonAsync<Dictionary<string, int>>();
            return ($"Batch-{testName}", stopwatch.ElapsedMilliseconds, availabilityResults?.Count == partnerEmails.Count);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Batch availability request failed for {testName}: {ex.Message}");
            return ($"Batch-{testName}", 0, false);
        }
    }

    private async Task<(string Type, long ElapsedMs, bool Success)> TestTimeframeAvailabilityRequestAsync(
        string partnerEmail, AvailabilityTimeframe timeframe, string testName)
    {
        try
        {
            var request = new PartnerTimeframeAvailabilityRequest
            {
                PartnerEmail = partnerEmail,
                Timeframe = timeframe.ToString()
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Scenario(x =>
            {
                x.Post.Json(request).ToUrl("/api/calendar/availability/timeframe");
                x.StatusCodeShouldBe(200);
            });
            stopwatch.Stop();
            
            var hasAvailability = await result.ReadAsJsonAsync<bool>();
            return ($"Timeframe-{testName}", stopwatch.ElapsedMilliseconds, true); // Any boolean result is success
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Timeframe availability request failed for {testName}: {ex.Message}");
            return ($"Timeframe-{testName}", 0, false);
        }
    }

    #endregion
}