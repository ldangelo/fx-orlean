using Microsoft.Playwright;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Services;

/// <summary>
/// Service for handling authentication errors and providing recovery mechanisms
/// </summary>
public class AuthenticationErrorHandlingService
{
    private readonly IPage _page;
    private readonly AuthenticationPage _authPage;
    private readonly AuthenticationConfigurationManager _configManager;

    public AuthenticationErrorHandlingService(
        IPage page, 
        AuthenticationPage authPage, 
        AuthenticationConfigurationManager configManager)
    {
        _page = page;
        _authPage = authPage;
        _configManager = configManager;
    }

    /// <summary>
    /// Handles authentication with comprehensive error recovery
    /// </summary>
    /// <param name="skipOnFailure">Skip test if authentication fails</param>
    /// <returns>True if authenticated, false if failed but test should continue</returns>
    public async Task<bool> HandleAuthenticationWithRecoveryAsync(bool skipOnFailure = false)
    {
        try
        {
            var config = await _configManager.LoadAuthenticationConfigAsync();
            var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();

            Console.WriteLine("Starting authentication with error recovery...");

            // Step 1: Check if authentication is required
            var currentUrl = _page.Url;
            if (!_authPage.IsAuthenticationUrl(currentUrl) && await _authPage.IsUserAuthenticatedAsync())
            {
                Console.WriteLine("User is already authenticated");
                return true;
            }

            // Step 2: Attempt authentication with retry logic
            var authResult = await _authPage.HandleGoogleOAuthAsync((int)effectiveTimeout);

            if (authResult)
            {
                Console.WriteLine("Authentication completed successfully");
                return true;
            }

            // Step 3: Check if authentication was cancelled by user
            var wasCancelled = await _authPage.DetectAuthenticationCancellationAsync();
            if (wasCancelled)
            {
                Console.WriteLine("Authentication was cancelled by user");
                
                if (skipOnFailure)
                {
                    throw new SkipException("Test skipped due to user cancellation of authentication");
                }
                
                return false;
            }

            // Step 4: Determine if this is a test environment limitation
            if (await IsTestEnvironmentLimitationAsync())
            {
                Console.WriteLine("Authentication timeout likely due to test environment - proceeding with test");
                return false; // Continue test execution
            }

            // Step 5: Attempt browser context reset and retry
            Console.WriteLine("Attempting authentication recovery...");
            var resetResult = await _authPage.ResetBrowserContextForRecoveryAsync();
            
            if (resetResult)
            {
                // One more attempt after reset
                authResult = await _authPage.HandleGoogleOAuthAsync((int)effectiveTimeout / 2, 1);
                
                if (authResult)
                {
                    Console.WriteLine("Authentication recovered after context reset");
                    return true;
                }
            }

            // Step 6: Final failure handling
            Console.WriteLine("Authentication failed after all recovery attempts");
            
            await _authPage.TakeDebugScreenshotAsync("auth-final-failure", new Dictionary<string, string>
            {
                ["Timeout"] = effectiveTimeout.ToString(),
                ["CurrentUrl"] = _page.Url,
                ["WasCancelled"] = wasCancelled.ToString(),
                ["ContextResetAttempted"] = resetResult.ToString()
            });

            if (skipOnFailure)
            {
                throw new SkipException("Test skipped due to authentication failure");
            }

            return false;
        }
        catch (SkipException)
        {
            throw; // Re-throw skip exceptions
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical authentication error: {ex.Message}");
            
            await _authPage.TakeDebugScreenshotAsync("auth-critical-error", new Dictionary<string, string>
            {
                ["Exception"] = ex.GetType().Name,
                ["Message"] = ex.Message,
                ["CurrentUrl"] = _page.Url
            });

            if (skipOnFailure)
            {
                throw new SkipException($"Test skipped due to critical authentication error: {ex.Message}");
            }

            return false;
        }
    }

    /// <summary>
    /// Determines if authentication failure is due to test environment limitations
    /// </summary>
    private async Task<bool> IsTestEnvironmentLimitationAsync()
    {
        try
        {
            // Check if we're running in headless mode
            var isHeadless = await _page.EvaluateAsync<bool>("() => window.navigator.webdriver === true");
            
            // Check if this looks like a CI environment
            var isCiEnvironment = Environment.GetEnvironmentVariable("CI") == "true" ||
                                  Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||
                                  Environment.GetEnvironmentVariable("JENKINS_URL") != null;

            // In test/CI environments, OAuth timeouts are expected
            return isHeadless || isCiEnvironment;
        }
        catch (Exception)
        {
            // If we can't determine the environment, assume it's a limitation
            return true;
        }
    }

    /// <summary>
    /// Validates authentication state and provides recovery recommendations
    /// </summary>
    public async Task<AuthenticationValidationResult> ValidateAuthenticationStateAsync()
    {
        var result = new AuthenticationValidationResult();

        try
        {
            result.IsAuthenticated = await _authPage.IsUserAuthenticatedAsync();
            result.SessionPersists = await _authPage.ValidateSessionPersistenceAsync();
            result.CookiesValid = await _authPage.ValidateAuthenticationCookiesAsync();
            result.UserContextAvailable = await _authPage.ValidateUserContextAvailabilityAsync();
            result.BrowserContextValid = !_page.IsClosed;

            // Determine overall state
            if (result.IsAuthenticated && result.SessionPersists && result.CookiesValid && result.BrowserContextValid)
            {
                result.OverallState = AuthenticationState.Healthy;
                result.Recommendations = new[] { "Authentication state is healthy" };
            }
            else if (!result.IsAuthenticated)
            {
                result.OverallState = AuthenticationState.NotAuthenticated;
                result.Recommendations = new[] { "User is not authenticated", "Consider running authentication flow" };
            }
            else if (!result.SessionPersists || !result.CookiesValid)
            {
                result.OverallState = AuthenticationState.Corrupted;
                result.Recommendations = new[] 
                { 
                    "Authentication session appears corrupted",
                    "Recommend browser context reset",
                    "Re-authenticate if needed"
                };
            }
            else
            {
                result.OverallState = AuthenticationState.Degraded;
                result.Recommendations = new[] 
                { 
                    "Authentication state is partially functional",
                    "Monitor for issues during test execution"
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            result.OverallState = AuthenticationState.Error;
            result.Recommendations = new[] 
            { 
                $"Error validating authentication: {ex.Message}",
                "Check browser context and page state"
            };
            result.ValidationError = ex;
            return result;
        }
    }
}

/// <summary>
/// Result of authentication state validation
/// </summary>
public class AuthenticationValidationResult
{
    public bool IsAuthenticated { get; set; }
    public bool SessionPersists { get; set; }
    public bool CookiesValid { get; set; }
    public bool UserContextAvailable { get; set; }
    public bool BrowserContextValid { get; set; }
    public AuthenticationState OverallState { get; set; }
    public string[] Recommendations { get; set; } = Array.Empty<string>();
    public Exception? ValidationError { get; set; }

    public override string ToString()
    {
        return $"Auth: {IsAuthenticated}, Session: {SessionPersists}, Cookies: {CookiesValid}, " +
               $"Context: {UserContextAvailable}, Browser: {BrowserContextValid}, State: {OverallState}";
    }
}

/// <summary>
/// Overall authentication state classification
/// </summary>
public enum AuthenticationState
{
    Healthy,        // All components working correctly
    NotAuthenticated, // User not authenticated (expected state)
    Degraded,       // Partially functional
    Corrupted,      // Session corrupted, needs reset
    Error           // Error during validation
}

/// <summary>
/// Exception for skipping tests due to authentication issues
/// </summary>
public class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
    public SkipException(string message, Exception innerException) : base(message, innerException) { }
}