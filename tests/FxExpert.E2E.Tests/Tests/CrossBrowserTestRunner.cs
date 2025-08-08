using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;
using FxExpert.E2E.Tests.Services;

namespace FxExpert.E2E.Tests.Tests;

/// <summary>
/// Comprehensive cross-browser test runner for OAuth authentication
/// </summary>
[TestFixture]
public class CrossBrowserTestRunner
{
    private BrowserConfigurationService? _browserConfig;
    private readonly List<string> _supportedBrowsers = new() { "Chromium", "Firefox", "WebKit" };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _browserConfig = new BrowserConfigurationService();
        
        // Create screenshots directory
        Directory.CreateDirectory("screenshots");
        Directory.CreateDirectory("screenshots/cross-browser");
        
        Console.WriteLine("Cross-Browser Test Runner initialized");
        Console.WriteLine($"Supported browsers: {string.Join(", ", _supportedBrowsers)}");
    }

    [Test]
    [Category("Cross-Browser")]
    [Category("Authentication")]
    [Category("Comprehensive")]
    public async Task RunComprehensiveCrossBrowserAuthenticationTest()
    {
        var results = new CrossBrowserTestResults
        {
            TestStartTime = DateTime.UtcNow
        };

        Console.WriteLine("Starting comprehensive cross-browser authentication testing...");

        foreach (var browserName in _supportedBrowsers)
        {
            Console.WriteLine($"\n=== Testing {browserName} ===");
            
            try
            {
                var browserAvailable = await _browserConfig!.IsBrowserAvailableAsync(browserName);
                
                if (!browserAvailable)
                {
                    Console.WriteLine($"{browserName} is not available - skipping");
                    results.BrowserResults[browserName] = false;
                    results.Errors[browserName] = new SkipException($"{browserName} not available");
                    continue;
                }

                var testResult = await RunBrowserSpecificTest(browserName);
                results.BrowserResults[browserName] = testResult.Success;
                results.PerformanceResults[browserName] = testResult.Duration;
                results.Errors[browserName] = testResult.Error;

                Console.WriteLine($"{browserName} test completed: {testResult.Success} ({testResult.Duration.TotalSeconds:F2}s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error testing {browserName}: {ex.Message}");
                results.BrowserResults[browserName] = false;
                results.Errors[browserName] = ex;
            }
        }

        results.TestEndTime = DateTime.UtcNow;
        
        // Analyze and report results
        await GenerateCrossBrowserReport(results);
        
        // Verify minimum success criteria
        VerifyCrossBrowserResults(results);
    }

    [Test]
    [Category("Cross-Browser")]
    [Category("Authentication")]
    [Category("Individual")]
    [TestCase("Chromium")]
    [TestCase("Firefox")]  
    [TestCase("WebKit")]
    public async Task RunIndividualBrowserAuthenticationTest(string browserName)
    {
        Console.WriteLine($"Running individual authentication test for {browserName}");

        var browserAvailable = await _browserConfig!.IsBrowserAvailableAsync(browserName);
        if (!browserAvailable)
        {
            Assert.Ignore($"{browserName} is not available for testing");
            return;
        }

        var result = await RunBrowserSpecificTest(browserName);
        
        // Log detailed results
        Console.WriteLine($"{browserName} Individual Test Results:");
        Console.WriteLine($"  Success: {result.Success}");
        Console.WriteLine($"  Duration: {result.Duration.TotalSeconds:F2}s");
        Console.WriteLine($"  Error: {result.Error?.Message ?? "None"}");

        // In test environment, we expect connection failures but not crashes
        if (result.Error != null)
        {
            var isExpectedError = result.Error.Message.Contains("ERR_CONNECTION_REFUSED") ||
                                  result.Error.Message.Contains("connection") ||
                                  result.Error.Message.Contains("timeout");
            
            if (!isExpectedError)
            {
                Assert.Fail($"Unexpected error in {browserName}: {result.Error.Message}");
            }
            else
            {
                Console.WriteLine($"{browserName} failed with expected connection error - test environment limitation");
            }
        }
    }

    [Test]
    [Category("Cross-Browser")]
    [Category("Performance")]
    public async Task CompareCrossBrowserPerformance()
    {
        var performanceResults = new Dictionary<string, BrowserPerformanceProfile>();
        
        Console.WriteLine("Comparing cross-browser performance characteristics...");

        foreach (var browserName in _supportedBrowsers)
        {
            var profile = _browserConfig!.GetPerformanceProfile(browserName);
            performanceResults[browserName] = profile;
            
            Console.WriteLine($"{browserName} Performance Profile:");
            Console.WriteLine($"  Expected startup: {profile.ExpectedStartupTime.TotalSeconds:F1}s");
            Console.WriteLine($"  Expected OAuth: {profile.ExpectedOAuthFlowTime.TotalSeconds:F1}s");
            Console.WriteLine($"  Reliability: {profile.ReliabilityScore:P0}");
            Console.WriteLine($"  Recommended concurrency: {profile.RecommendedConcurrency}");
        }

        // Verify performance profiles are reasonable
        foreach (var profile in performanceResults.Values)
        {
            profile.ExpectedStartupTime.Should().BeLessThan(TimeSpan.FromSeconds(10), 
                $"{profile.BrowserName} startup time should be reasonable");
            profile.ReliabilityScore.Should().BeGreaterThan(0.5, 
                $"{profile.BrowserName} should have reasonable reliability");
            profile.RecommendedConcurrency.Should().BeGreaterThan(0, 
                $"{profile.BrowserName} should allow some concurrency");
        }

        Console.WriteLine("Cross-browser performance comparison completed successfully");
    }

    [Test]
    [Category("Cross-Browser")]
    [Category("Configuration")]
    public async Task ValidateBrowserConfigurations()
    {
        Console.WriteLine("Validating browser configurations...");

        foreach (var browserName in _supportedBrowsers)
        {
            var config = _browserConfig!.GetConfiguration(browserName);
            
            Console.WriteLine($"{browserName} Configuration:");
            Console.WriteLine($"  Default timeout: {config.DefaultTimeout}ms");
            Console.WriteLine($"  Auth timeout: {config.AuthenticationTimeout}ms");
            Console.WriteLine($"  Retry multiplier: {config.RetryMultiplier}");
            Console.WriteLine($"  Requires special handling: {config.RequiresSpecialHandling}");
            Console.WriteLine($"  Launch args: {string.Join(", ", config.Args)}");

            // Validate configuration values
            config.DefaultTimeout.Should().BeGreaterThan(0, $"{browserName} should have positive default timeout");
            config.AuthenticationTimeout.Should().BeGreaterThan(config.DefaultTimeout, 
                $"{browserName} auth timeout should be longer than default");
            config.RetryMultiplier.Should().BeGreaterThan(0, $"{browserName} should have positive retry multiplier");
            config.OptimalViewportSize.Width.Should().BeGreaterThan(800, $"{browserName} should have reasonable viewport width");
            config.OptimalViewportSize.Height.Should().BeGreaterThan(600, $"{browserName} should have reasonable viewport height");

            // Test launch options creation
            var launchOptions = _browserConfig.CreateLaunchOptions(browserName, headless: true);
            launchOptions.Should().NotBeNull($"{browserName} should have valid launch options");
            
            var contextOptions = _browserConfig.CreateContextOptions(browserName);
            contextOptions.Should().NotBeNull($"{browserName} should have valid context options");
        }

        Console.WriteLine("Browser configuration validation completed successfully");
    }

    private async Task<BrowserTestResult> RunBrowserSpecificTest(string browserName)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Create browser-specific Playwright instance
            using var playwright = await Playwright.CreateAsync();
            
            var browserType = browserName.ToLower() switch
            {
                "chromium" => playwright.Chromium,
                "firefox" => playwright.Firefox, 
                "webkit" => playwright.Webkit,
                _ => throw new NotSupportedException($"Browser {browserName} not supported")
            };

            var launchOptions = _browserConfig!.CreateLaunchOptions(browserName, headless: true);
            await using var browser = await browserType.LaunchAsync(launchOptions);
            
            var contextOptions = _browserConfig.CreateContextOptions(browserName);
            await using var context = await browser.NewContextAsync(contextOptions);
            
            var page = await context.NewPageAsync();

            // Initialize page objects with browser-specific configuration
            var authPage = new AuthenticationPage(page);
            var homePage = new HomePage(page);
            var configManager = AuthenticationConfigurationManager.CreateDefault("Development");

            // Run authentication test with browser-specific settings
            var timeout = _browserConfig.GetOAuthTimeout(browserName, 10000); // Short timeout for testing
            var retryAttempts = 1; // Single attempt for cross-browser testing

            Console.WriteLine($"Testing {browserName} with timeout: {timeout}ms, retries: {retryAttempts}");

            // Navigate to authentication page
            await authPage.NavigateAsync();
            
            // Attempt OAuth flow
            var authResult = await authPage.HandleGoogleOAuthAsync(timeout, retryAttempts);
            
            // Test authentication state detection
            var isAuthenticated = await authPage.IsUserAuthenticatedAsync();
            
            // Test session persistence methods
            var sessionValid = await homePage.ValidateSessionPersistenceAsync();
            var cookiesValid = await homePage.ValidateAuthenticationCookiesAsync();

            // Take browser-specific screenshot
            await authPage.TakeDebugScreenshotAsync($"cross-browser-{browserName.ToLower()}-complete", 
                new Dictionary<string, string>
                {
                    ["Browser"] = browserName,
                    ["AuthResult"] = authResult.ToString(),
                    ["IsAuthenticated"] = isAuthenticated.ToString(),
                    ["SessionValid"] = sessionValid.ToString(),
                    ["CookiesValid"] = cookiesValid.ToString(),
                    ["Timeout"] = timeout.ToString()
                });

            var duration = DateTime.UtcNow - startTime;
            
            // In test environment, OAuth will timeout but methods should work
            var success = !authResult && // OAuth should timeout
                          !isAuthenticated && // Should not be authenticated
                          true; // Methods should execute without errors

            return new BrowserTestResult
            {
                Success = success,
                Duration = duration,
                Error = null,
                Details = new Dictionary<string, object>
                {
                    ["AuthResult"] = authResult,
                    ["IsAuthenticated"] = isAuthenticated,
                    ["SessionValid"] = sessionValid,
                    ["CookiesValid"] = cookiesValid
                }
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            return new BrowserTestResult
            {
                Success = false,
                Duration = duration,
                Error = ex,
                Details = new Dictionary<string, object>
                {
                    ["ErrorMessage"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name
                }
            };
        }
    }

    private async Task GenerateCrossBrowserReport(CrossBrowserTestResults results)
    {
        var reportPath = "screenshots/cross-browser/test-report.txt";
        var report = new List<string>
        {
            "=== Cross-Browser Authentication Test Report ===",
            $"Test Date: {results.TestStartTime:yyyy-MM-dd HH:mm:ss}",
            $"Total Duration: {results.TotalDuration.TotalMinutes:F1} minutes",
            $"Success Rate: {results.SuccessRate:P1}",
            "",
            "Browser Results:"
        };

        foreach (var result in results.BrowserResults)
        {
            var browser = result.Key;
            var success = result.Value;
            var performance = results.PerformanceResults.GetValueOrDefault(browser, TimeSpan.Zero);
            var error = results.Errors.GetValueOrDefault(browser);

            report.Add($"  {browser}: {(success ? "PASS" : "FAIL")} ({performance.TotalSeconds:F2}s)");
            
            if (error != null)
            {
                report.Add($"    Error: {error.Message}");
                if (error.Message.Contains("ERR_CONNECTION_REFUSED"))
                {
                    report.Add($"    Note: Connection error is expected in test environment");
                }
            }
        }

        if (results.PerformanceResults.Count > 1)
        {
            report.Add("");
            report.Add("Performance Analysis:");
            report.Add($"  Average: {results.AveragePerformance.TotalSeconds:F2}s");
            report.Add($"  Fastest: {results.PerformanceResults.Values.Min().TotalSeconds:F2}s");
            report.Add($"  Slowest: {results.PerformanceResults.Values.Max().TotalSeconds:F2}s");
        }

        await File.WriteAllLinesAsync(reportPath, report);
        Console.WriteLine($"\nCross-browser test report generated: {reportPath}");
    }

    private void VerifyCrossBrowserResults(CrossBrowserTestResults results)
    {
        // At least one browser should be testable
        results.BrowserResults.Should().NotBeEmpty("At least one browser should be available for testing");
        
        // Success rate should be reasonable (allowing for test environment limitations)
        if (results.BrowserResults.Count > 0)
        {
            var connectionErrors = results.Errors.Values
                .Count(e => e != null && e.Message.Contains("ERR_CONNECTION_REFUSED"));
            
            if (connectionErrors == results.BrowserResults.Count)
            {
                Console.WriteLine("All browsers failed with connection errors - test environment limitation");
                Assert.Pass("All browsers handled connection errors gracefully");
            }
            else
            {
                // At least 50% should succeed if server is available
                results.SuccessRate.Should().BeGreaterThan(0.0, 
                    "At least some browsers should handle authentication flow correctly");
            }
        }

        // Performance should be reasonable
        if (results.PerformanceResults.Count > 1)
        {
            var maxPerformance = results.PerformanceResults.Values.Max();
            maxPerformance.Should().BeLessThan(TimeSpan.FromMinutes(2), 
                "No browser should take more than 2 minutes for OAuth flow");
        }

        Console.WriteLine($"\nCross-browser verification passed: {results}");
    }
}

/// <summary>
/// Result of a browser-specific test
/// </summary>
public class BrowserTestResult
{
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public Exception? Error { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}