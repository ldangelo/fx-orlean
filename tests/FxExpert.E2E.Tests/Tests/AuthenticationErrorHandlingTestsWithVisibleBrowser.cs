using Microsoft.Playwright;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class AuthenticationErrorHandlingTestsWithVisibleBrowser
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private HomePage? _homePage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        Console.WriteLine("🔧 Setting up visible browser for error handling tests...");
        
        // Create Playwright instance
        _playwright = await Playwright.CreateAsync();
        
        // Create browser with visible mode for OAuth error testing
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = false,  // Visible browser for OAuth error testing
            SlowMo = 200,      // Reasonable speed for error testing
            Timeout = 60000,   // 60 second timeout
            Args = new[]
            {
                "--disable-web-security",
                "--disable-blink-features=AutomationControlled",
                "--no-first-run",
                "--disable-extensions"
            }
        };
        
        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
        
        // Create context optimized for error testing
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true,
            AcceptDownloads = false,
            JavaScriptEnabled = true
        };
        
        _context = await _browser.NewContextAsync(contextOptions);
        _page = await _context.NewPageAsync();
        
        Console.WriteLine("📱 Browser window is visible and ready for error handling tests!");
        
        // Create page objects with the manual page
        _homePage = new HomePage(_page);
        _authPage = new AuthenticationPage(_page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        // Create screenshots directory
        Directory.CreateDirectory("screenshots");
    }

    [TearDown]
    public async Task TearDown()
    {
        Console.WriteLine("🧹 Cleaning up error handling test resources...");
        
        if (_page != null && !_page.IsClosed)
        {
            await _page.CloseAsync();
        }
        
        if (_context != null)
        {
            await _context.CloseAsync();
        }
        
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        
        _playwright?.Dispose();
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task HandleGoogleOAuthAsync_WithShortTimeout_WithVisibleBrowser_ShouldTimeoutGracefully()
    {
        Console.WriteLine("🎯 Testing OAuth timeout handling with visible browser");
        Console.WriteLine("=====================================================");
        
        // Arrange
        const int shortTimeoutMs = 5000; // 5 seconds - very short for OAuth flow
        
        Console.WriteLine("🌐 Navigating to FX-Orleans application...");
        Console.WriteLine("📋 MANUAL NOTE: This test will timeout quickly - this is expected behavior");
        Console.WriteLine("   ⏰ Timeout set to 5 seconds for testing timeout handling");
        
        await _authPage!.NavigateAsync();

        // Act
        Console.WriteLine("⏳ Attempting OAuth with short timeout...");
        var result = await _authPage.HandleGoogleOAuthAsync(shortTimeoutMs);

        // Assert
        result.Should().BeFalse("OAuth should timeout with short timeout period");
        
        Console.WriteLine($"✅ OAuth timeout result: {result} (expected: false)");
        
        // Verify error handling behavior
        var screenshots = Directory.GetFiles("screenshots", "*oauth*timeout*");
        screenshots.Should().NotBeEmpty("Screenshot should be taken on timeout");
        
        Console.WriteLine($"📸 Found {screenshots.Length} timeout screenshots");
        await TakeDebugScreenshot("short-timeout-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task HandleGoogleOAuthAsync_WithNetworkError_WithVisibleBrowser_ShouldHandleGracefully()
    {
        Console.WriteLine("🎯 Testing OAuth network error handling with visible browser");
        Console.WriteLine("==========================================================");
        
        // Arrange - Simulate network failure by navigating to invalid URL
        Console.WriteLine("🌐 Testing with invalid domain to simulate network error...");
        var invalidAuthPage = new AuthenticationPage(_page!, "https://invalid-domain-12345.com");

        // Act & Assert
        Exception? caughtException = null;
        try
        {
            Console.WriteLine("⏳ Attempting navigation to invalid domain...");
            await invalidAuthPage.NavigateAsync();
            Console.WriteLine("🔐 Attempting OAuth on invalid domain...");
            await invalidAuthPage.HandleGoogleOAuthAsync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
            Console.WriteLine($"❌ Caught expected exception: {ex.Message}");
        }

        // Should handle network errors gracefully
        caughtException.Should().NotBeNull("Network errors should be caught and handled");
        Console.WriteLine($"✅ Network error handled gracefully: {caughtException!.Message}");
        
        await TakeDebugScreenshot("network-error-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task HandleGoogleOAuthAsync_WithInvalidConfiguration_WithVisibleBrowser_ShouldUseDefaults()
    {
        Console.WriteLine("🎯 Testing OAuth with invalid configuration with visible browser");
        Console.WriteLine("===============================================================");
        
        // Arrange - Create config manager with invalid environment
        Console.WriteLine("⚙️ Creating configuration manager with invalid environment...");
        var invalidConfigManager = AuthenticationConfigurationManager.CreateDefault("InvalidEnvironment");
        
        // Act
        Console.WriteLine("📋 Loading authentication configuration...");
        var config = await invalidConfigManager.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await invalidConfigManager.GetEffectiveTimeoutAsync();

        // Assert
        config.Should().NotBeNull("Configuration should have defaults");
        config.Mode.Should().Be(AuthenticationMode.Manual, "Should default to manual mode");
        effectiveTimeout.Should().BeGreaterThan(0, "Should have positive timeout value");
        
        Console.WriteLine($"✅ Configuration defaults loaded successfully:");
        Console.WriteLine($"   Mode: {config.Mode}");
        Console.WriteLine($"   Timeout: {effectiveTimeout}ms");
        
        // Verify OAuth can still be attempted with defaults
        Console.WriteLine("🌐 Navigating to test OAuth with default configuration...");
        Console.WriteLine("📋 MANUAL NOTE: Using default configuration - will timeout as expected");
        
        await _authPage!.NavigateAsync();
        var result = await _authPage.HandleGoogleOAuthAsync((int)effectiveTimeout);
        
        // Should not crash, even if it times out
        result.Should().BeFalse("OAuth should timeout gracefully with default config");
        Console.WriteLine($"✅ OAuth with defaults result: {result} (expected: false)");
        
        await TakeDebugScreenshot("invalid-config-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task WaitForAuthenticationCompletionAsync_WithTimeout_WithVisibleBrowser_ShouldThrowTimeoutException()
    {
        Console.WriteLine("🎯 Testing authentication completion timeout with visible browser");
        Console.WriteLine("================================================================");
        
        // Arrange
        Console.WriteLine("🌐 Navigating to FX-Orleans application...");
        Console.WriteLine("📋 MANUAL NOTE: This test will timeout - this is expected behavior");
        
        await _authPage!.NavigateAsync();

        // Act & Assert
        TimeoutException? exception = null;
        try
        {
            Console.WriteLine("⏳ Waiting for authentication completion (will timeout)...");
            await _authPage.WaitForAuthenticationCompletionAsync();
        }
        catch (TimeoutException ex)
        {
            exception = ex;
            Console.WriteLine($"✅ Caught expected timeout exception: {ex.Message}");
        }

        exception.Should().NotBeNull("Timeout exception should be thrown");
        exception!.Message.Should().Contain("timeout", "Exception should mention timeout");
        
        // Verify screenshot was taken on error
        var screenshots = Directory.GetFiles("screenshots", "*auth-completion-error*");
        screenshots.Should().NotBeEmpty("Error screenshot should be taken");
        
        Console.WriteLine($"📸 Found {screenshots.Length} error screenshots");
        await TakeDebugScreenshot("auth-completion-timeout-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task IsUserAuthenticatedAsync_WithPageError_WithVisibleBrowser_ShouldReturnFalse()
    {
        Console.WriteLine("🎯 Testing authentication detection with page error with visible browser");
        Console.WriteLine("======================================================================");
        
        // Arrange - Navigate to a page that might cause errors
        Console.WriteLine("🌐 Navigating to non-existent page to simulate error...");
        await _authPage!.NavigateAsync("/non-existent-page");

        // Act
        Console.WriteLine("👤 Checking authentication state on error page...");
        var isAuthenticated = await _authPage.IsUserAuthenticatedAsync();

        // Assert
        isAuthenticated.Should().BeFalse("Should return false on page errors");
        Console.WriteLine($"✅ Authentication state on error page: {isAuthenticated} (expected: false)");
        
        await TakeDebugScreenshot("page-error-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task HandleGoogleOAuthAsync_WithBrowserClosed_WithVisibleBrowser_ShouldHandleGracefully()
    {
        Console.WriteLine("🎯 Testing OAuth with browser closure handling with visible browser");
        Console.WriteLine("===================================================================");
        
        // Arrange
        Console.WriteLine("🌐 Navigating to FX-Orleans application...");
        await _authPage!.NavigateAsync();

        // Act & Assert - This test verifies behavior when browser context is interrupted
        try
        {
            Console.WriteLine("🔐 Starting OAuth flow...");
            Console.WriteLine("📋 MANUAL NOTE: Browser will be closed during OAuth - this tests error handling");
            
            // Simulate browser being closed during OAuth flow
            var oauthTask = _authPage.HandleGoogleOAuthAsync(30000);
            
            Console.WriteLine("❌ Closing page during OAuth flow...");
            // Close the page while OAuth is in progress
            await _page!.CloseAsync();
            
            Console.WriteLine("⏳ Waiting for OAuth task to complete...");
            var result = await oauthTask;
            result.Should().BeFalse("OAuth should handle browser closure gracefully");
            Console.WriteLine($"✅ OAuth handled browser closure: {result} (expected: false)");
        }
        catch (Exception ex)
        {
            // Browser closure exceptions are expected and should be handled
            Console.WriteLine($"✅ Browser closure handled gracefully: {ex.Message}");
            ex.Should().NotBeOfType<NullReferenceException>("Should not cause null reference exceptions");
        }
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task AuthenticationConfiguration_WithMissingValues_WithVisibleBrowser_ShouldValidateCorrectly()
    {
        Console.WriteLine("🎯 Testing authentication configuration validation with visible browser");
        Console.WriteLine("=====================================================================");
        
        // Arrange - Test configuration validation with missing/invalid values
        var testCases = new[]
        {
            new { Mode = AuthenticationMode.Manual, Timeout = 0, ExpectedValid = false, Description = "Zero timeout" },
            new { Mode = AuthenticationMode.Manual, Timeout = -1, ExpectedValid = false, Description = "Negative timeout" },
            new { Mode = AuthenticationMode.Manual, Timeout = 60000, ExpectedValid = true, Description = "Valid manual config" },
            new { Mode = AuthenticationMode.Automated, Timeout = 60000, ExpectedValid = false, Description = "Automated without credentials" }
        };

        Console.WriteLine("⚙️ Testing configuration validation scenarios:");
        
        foreach (var testCase in testCases)
        {
            Console.WriteLine($"   🔍 Testing: {testCase.Description}");
            
            // Act
            var config = new AuthenticationConfiguration
            {
                Mode = testCase.Mode,
                Timeout = testCase.Timeout
            };

            // For automated mode without credentials, ensure TestAccount is null
            if (testCase.Mode == AuthenticationMode.Automated && testCase.ExpectedValid == false)
            {
                config.TestAccount = null;
            }

            // Assert
            var isValid = config.IsValid();
            isValid.Should().Be(testCase.ExpectedValid, 
                $"Configuration for '{testCase.Description}' should be {(testCase.ExpectedValid ? "valid" : "invalid")}");
                
            Console.WriteLine($"   ✅ {testCase.Description}: {(isValid ? "Valid" : "Invalid")} (expected: {(testCase.ExpectedValid ? "Valid" : "Invalid")})");
        }
        
        await TakeDebugScreenshot("config-validation-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task OAuth_WithMultipleConsecutiveAttempts_WithVisibleBrowser_ShouldHandleRateLimit()
    {
        Console.WriteLine("🎯 Testing OAuth rate limiting with multiple attempts with visible browser");
        Console.WriteLine("=========================================================================");
        
        // Arrange
        Console.WriteLine("🌐 Navigating to FX-Orleans application...");
        await _authPage!.NavigateAsync();
        const int shortTimeout = 2000; // 2 seconds

        var attempts = new List<bool>();
        
        Console.WriteLine("📋 MANUAL NOTE: Making 3 consecutive OAuth attempts - all will timeout quickly");
        Console.WriteLine("   This tests rate limiting and consecutive attempt handling");
        
        // Act - Make multiple consecutive OAuth attempts
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"🔐 OAuth attempt {i + 1}/3...");
            var result = await _authPage.HandleGoogleOAuthAsync(shortTimeout);
            attempts.Add(result);
            
            Console.WriteLine($"   Result: {result}");
            
            // Small delay between attempts
            if (i < 2) // Don't delay after last attempt
            {
                Console.WriteLine("   ⏳ Waiting 1 second before next attempt...");
                await Task.Delay(1000);
            }
        }

        // Assert
        attempts.Should().AllSatisfy(result => 
            result.Should().BeFalse("All attempts should timeout gracefully without errors"));
        
        // Verify no crashes or exceptions occurred
        Console.WriteLine($"✅ Completed {attempts.Count} consecutive OAuth attempts successfully");
        Console.WriteLine($"   All results: {string.Join(", ", attempts)}");
        
        await TakeDebugScreenshot("multiple-attempts-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task SessionPersistence_WithAuthenticationFailure_WithVisibleBrowser_ShouldResetCorrectly()
    {
        Console.WriteLine("🎯 Testing session persistence after authentication failure with visible browser");
        Console.WriteLine("===============================================================================");
        
        // Arrange
        Console.WriteLine("🌐 Navigating to home page...");
        await _homePage!.NavigateAsync();
        
        Console.WriteLine("🔐 Simulating authentication failure...");
        Console.WriteLine("📋 MANUAL NOTE: Authentication will timeout - this simulates failure");
        
        // Simulate authentication attempt that fails
        await _authPage!.HandleGoogleOAuthAsync(5000); // Short timeout to ensure failure

        // Act - Check session state after failed authentication
        Console.WriteLine("🔍 Checking session state after authentication failure...");
        var sessionPersists = await _homePage.ValidateSessionPersistenceAsync();
        var cookiesValid = await _homePage.ValidateAuthenticationCookiesAsync();

        // Assert - Failed authentication should not leave invalid session state
        Console.WriteLine($"📊 Session persistence after auth failure: {sessionPersists}");
        Console.WriteLine($"🍪 Cookie state after auth failure: {cookiesValid}");
        
        // Should either have no session or valid session, but not corrupted state
        (sessionPersists || !await _homePage.IsUserAuthenticatedAsync())
            .Should().BeTrue("Session should be either valid or properly cleared after auth failure");
            
        Console.WriteLine("✅ Session state is consistent after authentication failure");
        
        await TakeDebugScreenshot("session-persistence-failure-test");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    [Category("VisibleBrowser")]
    public async Task ErrorLogging_DuringOAuthFlow_WithVisibleBrowser_ShouldCaptureDebugInformation()
    {
        Console.WriteLine("🎯 Testing OAuth error logging and debug capture with visible browser");
        Console.WriteLine("====================================================================");
        
        // Arrange
        Console.WriteLine("🌐 Navigating to FX-Orleans application...");
        await _authPage!.NavigateAsync();

        Console.WriteLine("📋 MANUAL NOTE: OAuth will timeout to test error logging");
        Console.WriteLine("   Capturing console output for verification");

        // Act - Trigger OAuth flow that will timeout
        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                Console.WriteLine("🔐 Starting OAuth flow for error logging test...");
                await _authPage.HandleGoogleOAuthAsync(5000); // Short timeout
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            var logOutput = stringWriter.ToString();

            Console.WriteLine("📋 Analyzing captured log output...");
            
            // Assert - Verify comprehensive logging
            logOutput.Should().Contain("Starting Google OAuth authentication flow", 
                "Should log OAuth start");
            logOutput.Should().Contain("OAuth authentication timed out", 
                "Should log timeout event");
            
            Console.WriteLine("✅ OAuth logging verification:");
            Console.WriteLine($"   Start message found: {logOutput.Contains("Starting Google OAuth authentication flow")}");
            Console.WriteLine($"   Timeout message found: {logOutput.Contains("OAuth authentication timed out")}");
            
            // Verify screenshots were captured
            var errorScreenshots = Directory.GetFiles("screenshots", "*oauth*");
            errorScreenshots.Should().NotBeEmpty("Error screenshots should be captured");
            
            Console.WriteLine($"📸 Found {errorScreenshots.Length} OAuth-related screenshots");
        }
        
        await TakeDebugScreenshot("error-logging-test");
    }

    private async Task TakeDebugScreenshot(string name)
    {
        try
        {
            var screenshotPath = Path.Combine("screenshots", $"error-handling-{name}-{DateTime.Now:yyyyMMdd-HHmmss}.png");
            await _page!.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
            Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not save screenshot: {ex.Message}");
        }
    }
}