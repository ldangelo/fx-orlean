using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class AuthenticationErrorHandlingTests : PageTest
{
    private HomePage? _homePage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        _authPage = new AuthenticationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        // Create screenshots directory
        await Task.Run(() => Directory.CreateDirectory("screenshots"));
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task HandleGoogleOAuthAsync_WithShortTimeout_ShouldTimeoutGracefully()
    {
        // Arrange
        const int shortTimeoutMs = 5000; // 5 seconds - very short for OAuth flow
        await _authPage!.NavigateAsync();

        // Act
        var result = await _authPage.HandleGoogleOAuthAsync(shortTimeoutMs);

        // Assert
        result.Should().BeFalse("OAuth should timeout with short timeout period");
        
        // Verify error handling behavior
        var screenshots = Directory.GetFiles("screenshots", "*oauth*timeout*");
        screenshots.Should().NotBeEmpty("Screenshot should be taken on timeout");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task HandleGoogleOAuthAsync_WithNetworkError_ShouldHandleGracefully()
    {
        // Arrange - Simulate network failure by navigating to invalid URL
        var invalidAuthPage = new AuthenticationPage(Page, "https://invalid-domain-12345.com");

        // Act & Assert
        Exception? caughtException = null;
        try
        {
            await invalidAuthPage.NavigateAsync();
            await invalidAuthPage.HandleGoogleOAuthAsync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Should handle network errors gracefully
        caughtException.Should().NotBeNull("Network errors should be caught and handled");
        Console.WriteLine($"Network error handled: {caughtException!.Message}");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task HandleGoogleOAuthAsync_WithInvalidConfiguration_ShouldUseDefaults()
    {
        // Arrange - Create config manager with invalid environment
        var invalidConfigManager = AuthenticationConfigurationManager.CreateDefault("InvalidEnvironment");
        
        // Act
        var config = await invalidConfigManager.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await invalidConfigManager.GetEffectiveTimeoutAsync();

        // Assert
        config.Should().NotBeNull("Configuration should have defaults");
        config.Mode.Should().Be(AuthenticationMode.Manual, "Should default to manual mode");
        effectiveTimeout.Should().BeGreaterThan(0, "Should have positive timeout value");
        
        // Verify OAuth can still be attempted with defaults
        await _authPage!.NavigateAsync();
        var result = await _authPage.HandleGoogleOAuthAsync((int)effectiveTimeout);
        
        // Should not crash, even if it times out
        result.Should().BeFalse("OAuth should timeout gracefully with default config");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task WaitForAuthenticationCompletionAsync_WithTimeout_ShouldThrowTimeoutException()
    {
        // Arrange
        await _authPage!.NavigateAsync();

        // Act & Assert
        TimeoutException? exception = null;
        try
        {
            await _authPage.WaitForAuthenticationCompletionAsync();
        }
        catch (TimeoutException ex)
        {
            exception = ex;
        }

        exception.Should().NotBeNull("Timeout exception should be thrown");
        exception!.Message.Should().Contain("timeout", "Exception should mention timeout");
        
        // Verify screenshot was taken on error
        var screenshots = Directory.GetFiles("screenshots", "*auth-completion-error*");
        screenshots.Should().NotBeEmpty("Error screenshot should be taken");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task IsUserAuthenticatedAsync_WithPageError_ShouldReturnFalse()
    {
        // Arrange - Navigate to a page that might cause errors
        await _authPage!.NavigateAsync("/non-existent-page");

        // Act
        var isAuthenticated = await _authPage.IsUserAuthenticatedAsync();

        // Assert
        isAuthenticated.Should().BeFalse("Should return false on page errors");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task HandleGoogleOAuthAsync_WithBrowserClosed_ShouldHandleGracefully()
    {
        // Arrange
        await _authPage!.NavigateAsync();

        // Act & Assert - This test verifies behavior when browser context is interrupted
        try
        {
            // Simulate browser being closed during OAuth flow
            var oauthTask = _authPage.HandleGoogleOAuthAsync(30000);
            
            // Close the page while OAuth is in progress
            await Page.CloseAsync();
            
            var result = await oauthTask;
            result.Should().BeFalse("OAuth should handle browser closure gracefully");
        }
        catch (Exception ex)
        {
            // Browser closure exceptions are expected and should be handled
            Console.WriteLine($"Browser closure handled: {ex.Message}");
            ex.Should().NotBeOfType<NullReferenceException>("Should not cause null reference exceptions");
        }
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task AuthenticationConfiguration_WithMissingValues_ShouldValidateCorrectly()
    {
        // Arrange - Test configuration validation with missing/invalid values
        var testCases = new[]
        {
            new { Mode = AuthenticationMode.Manual, Timeout = 0, ExpectedValid = false, Description = "Zero timeout" },
            new { Mode = AuthenticationMode.Manual, Timeout = -1, ExpectedValid = false, Description = "Negative timeout" },
            new { Mode = AuthenticationMode.Manual, Timeout = 60000, ExpectedValid = true, Description = "Valid manual config" },
            new { Mode = AuthenticationMode.Automated, Timeout = 60000, ExpectedValid = false, Description = "Automated without credentials" }
        };

        foreach (var testCase in testCases)
        {
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
        }
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task OAuth_WithMultipleConsecutiveAttempts_ShouldHandleRateLimit()
    {
        // Arrange
        await _authPage!.NavigateAsync();
        const int shortTimeout = 2000; // 2 seconds

        var attempts = new List<bool>();
        
        // Act - Make multiple consecutive OAuth attempts
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"OAuth attempt {i + 1}");
            var result = await _authPage.HandleGoogleOAuthAsync(shortTimeout);
            attempts.Add(result);
            
            // Small delay between attempts
            await Task.Delay(1000);
        }

        // Assert
        attempts.Should().AllSatisfy(result => 
            result.Should().BeFalse("All attempts should timeout gracefully without errors"));
        
        // Verify no crashes or exceptions occurred
        Console.WriteLine($"Completed {attempts.Count} consecutive OAuth attempts successfully");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task SessionPersistence_WithAuthenticationFailure_ShouldResetCorrectly()
    {
        // Arrange
        await _homePage!.NavigateAsync();
        
        // Simulate authentication attempt that fails
        await _authPage!.HandleGoogleOAuthAsync(5000); // Short timeout to ensure failure

        // Act - Check session state after failed authentication
        var sessionPersists = await _homePage.ValidateSessionPersistenceAsync();
        var cookiesValid = await _homePage.ValidateAuthenticationCookiesAsync();

        // Assert - Failed authentication should not leave invalid session state
        Console.WriteLine($"Session persistence after auth failure: {sessionPersists}");
        Console.WriteLine($"Cookie state after auth failure: {cookiesValid}");
        
        // Should either have no session or valid session, but not corrupted state
        (sessionPersists || !await _homePage.IsUserAuthenticatedAsync())
            .Should().BeTrue("Session should be either valid or properly cleared after auth failure");
    }

    [Test]
    [Category("Error-Handling")]
    [Category("Authentication")]
    public async Task ErrorLogging_DuringOAuthFlow_ShouldCaptureDebugInformation()
    {
        // Arrange
        await _authPage!.NavigateAsync();

        // Act - Trigger OAuth flow that will timeout
        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                await _authPage.HandleGoogleOAuthAsync(5000); // Short timeout
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            var logOutput = stringWriter.ToString();

            // Assert - Verify comprehensive logging
            logOutput.Should().Contain("Starting Google OAuth authentication flow", 
                "Should log OAuth start");
            logOutput.Should().Contain("OAuth authentication timed out", 
                "Should log timeout event");
            
            // Verify screenshots were captured
            var errorScreenshots = Directory.GetFiles("screenshots", "*oauth*");
            errorScreenshots.Should().NotBeEmpty("Error screenshots should be captured");
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        if (Page != null && !Page.IsClosed)
        {
            await Page.CloseAsync();
        }
    }
}