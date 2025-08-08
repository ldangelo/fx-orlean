using Microsoft.Playwright;
using FluentAssertions;
using System.Net.Http;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class AuthenticationPage : BasePage
{
    private const int DEFAULT_TIMEOUT_MS = 60000;
    private const int DEFAULT_RETRY_ATTEMPTS = 3;
    private const int AUTHENTICATION_CHECK_INTERVAL_MS = 1000;

    public AuthenticationPage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    /// <summary>
    /// Handles Google OAuth authentication flow with manual user intervention
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds for OAuth completion</param>
    /// <param name="retryAttempts">Number of retry attempts for transient failures</param>
    /// <returns>True if authentication completes successfully, false otherwise</returns>
    public async Task<bool> HandleGoogleOAuthAsync(int timeoutMs = DEFAULT_TIMEOUT_MS, int retryAttempts = DEFAULT_RETRY_ATTEMPTS)
    {
        for (int attempt = 1; attempt <= retryAttempts; attempt++)
        {
            try
            {
                Console.WriteLine($"Starting Google OAuth authentication flow (attempt {attempt}/{retryAttempts})...");
                
                // Check for browser context validity
                if (Page.IsClosed)
                {
                    Console.WriteLine("Browser page is closed - cannot proceed with OAuth");
                    return false;
                }

                // First, check if we're already authenticated
                if (await IsUserAuthenticatedAsync())
                {
                    Console.WriteLine("User is already authenticated");
                    return true;
                }

                // Look for and click Google sign-in button if present
                await ClickGoogleSignInButtonIfPresent();

                // Wait for OAuth flow to complete or timeout
                var result = await WaitForOAuthCompletionWithTimeout(timeoutMs);
                
                if (result)
                {
                    Console.WriteLine("OAuth authentication completed successfully");
                    return true;
                }

                // If this was the last attempt, don't retry
                if (attempt == retryAttempts)
                {
                    Console.WriteLine("OAuth authentication failed after all retry attempts");
                    await TakeScreenshotAsync("oauth-final-timeout");
                    return false;
                }

                // Wait before retry
                Console.WriteLine($"OAuth attempt {attempt} timed out, waiting before retry...");
                await Task.Delay(2000); // 2 second delay between retries
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                Console.WriteLine($"Transient OAuth error on attempt {attempt}: {ex.Message}");
                await TakeScreenshotAsync($"oauth-transient-error-{attempt}");
                
                if (attempt == retryAttempts)
                {
                    Console.WriteLine("OAuth authentication failed with transient errors after all retries");
                    return false;
                }

                // Wait longer for transient errors
                await Task.Delay(5000); // 5 second delay for transient errors
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical OAuth authentication error: {ex.Message}");
                await TakeScreenshotAsync($"oauth-critical-error-{attempt}");
                
                // Don't retry for critical errors
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Waits for authentication completion by monitoring page state changes
    /// </summary>
    public async Task WaitForAuthenticationCompletionAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT_MS);

            Console.WriteLine("Waiting for authentication completion...");

            while (DateTime.UtcNow - startTime < timeout)
            {
                if (await IsUserAuthenticatedAsync())
                {
                    Console.WriteLine("Authentication completion detected");
                    return;
                }

                await Task.Delay(AUTHENTICATION_CHECK_INTERVAL_MS);
            }

            throw new TimeoutException($"Authentication completion timeout after {DEFAULT_TIMEOUT_MS}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error waiting for authentication completion: {ex.Message}");
            await TakeScreenshotAsync("auth-completion-error");
            throw;
        }
    }

    /// <summary>
    /// Detects if user is currently authenticated by checking UI elements
    /// </summary>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public override async Task<bool> IsUserAuthenticatedAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            
            // If we're on a login page (contains sign-in), we're not authenticated
            var pageTitle = await Page.TitleAsync();
            if (pageTitle.ToLower().Contains("sign in"))
            {
                return false;
            }

            // Check for authenticated UI indicators
            var authenticatedIndicators = new[]
            {
                "[data-testid='user-menu']",
                "[data-testid='authenticated-user']", 
                "text=Dashboard",
                "text=My Profile",
                "text=Logout",
                "text=Sign Out"
            };

            foreach (var indicator in authenticatedIndicators)
            {
                try
                {
                    var element = await Page.WaitForSelectorAsync(indicator, new() { Timeout = 2000 });
                    if (element != null)
                    {
                        return true;
                    }
                }
                catch (TimeoutException)
                {
                    // Continue to next indicator
                }
            }

            // Check if URL indicates authenticated state (not on login/auth pages)
            return !IsAuthenticationUrl(currentUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking authentication state: {ex.Message}");
            return false;
        }
    }

    // Private helper methods for OAuth flow handling
    private async Task ClickGoogleSignInButtonIfPresent()
    {
        try
        {
            var googleSignInSelectors = new[]
            {
                "text=Sign in with Google",
                "[data-testid='google-signin']",
                ".google-signin-button",
                "button:has-text('Google')"
            };

            foreach (var selector in googleSignInSelectors)
            {
                try
                {
                    var button = await Page.WaitForSelectorAsync(selector, new() { Timeout = 5000 });
                    if (button != null)
                    {
                        Console.WriteLine($"Clicking Google sign-in button: {selector}");
                        await button.ClickAsync();
                        return;
                    }
                }
                catch (TimeoutException)
                {
                    // Try next selector
                }
            }

            Console.WriteLine("Google sign-in button not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clicking Google sign-in button: {ex.Message}");
        }
    }

    private bool IsGoogleOAuthUrl(string url)
    {
        return url.Contains("accounts.google.com") || 
               url.Contains("oauth2/auth") ||
               url.Contains("google.com/oauth");
    }

    public new bool IsAuthenticationUrl(string url)
    {
        var authUrls = new[] { "/auth", "/login", "/signin", "accounts.google.com", "oauth" };
        return authUrls.Any(authPath => url.ToLower().Contains(authPath.ToLower()));
    }

    /// <summary>
    /// Waits for OAuth completion with timeout and detailed progress tracking
    /// </summary>
    private async Task<bool> WaitForOAuthCompletionWithTimeout(int timeoutMs)
    {
        var startTime = DateTime.UtcNow;
        var lastStatusTime = startTime;
        var inOAuthFlow = false;

        while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(timeoutMs))
        {
            try
            {
                // Check for browser context validity
                if (Page.IsClosed)
                {
                    Console.WriteLine("Browser page closed during OAuth flow");
                    return false;
                }

                // Check if authentication completed
                if (await IsUserAuthenticatedAsync())
                {
                    Console.WriteLine("OAuth authentication completed successfully");
                    return true;
                }

                // Check if we're in OAuth flow (redirected to Google)
                var currentUrl = Page.Url;
                var currentlyInOAuthFlow = IsGoogleOAuthUrl(currentUrl);

                if (currentlyInOAuthFlow && !inOAuthFlow)
                {
                    Console.WriteLine($"OAuth flow detected. Waiting for user to complete authentication at: {currentUrl}");
                    Console.WriteLine("Please complete the Google authentication in the browser...");
                    inOAuthFlow = true;
                    lastStatusTime = DateTime.UtcNow;
                }
                else if (!currentlyInOAuthFlow && inOAuthFlow)
                {
                    Console.WriteLine("OAuth flow completed - checking authentication status...");
                    inOAuthFlow = false;
                }

                // Provide periodic status updates
                if (DateTime.UtcNow - lastStatusTime > TimeSpan.FromSeconds(10))
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    var remaining = TimeSpan.FromMilliseconds(timeoutMs) - elapsed;
                    Console.WriteLine($"OAuth waiting... {remaining.TotalSeconds:F0}s remaining (URL: {currentUrl})");
                    lastStatusTime = DateTime.UtcNow;
                }

                await Task.Delay(AUTHENTICATION_CHECK_INTERVAL_MS);
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                Console.WriteLine($"Transient error during OAuth wait: {ex.Message}");
                await Task.Delay(AUTHENTICATION_CHECK_INTERVAL_MS * 2); // Wait longer on transient errors
            }
        }

        Console.WriteLine("OAuth authentication timed out");
        return false;
    }

    /// <summary>
    /// Determines if an exception is transient and should be retried
    /// </summary>
    private bool IsTransientError(Exception ex)
    {
        var transientErrorMessages = new[]
        {
            "timeout",
            "network",
            "connection",
            "temporary",
            "service unavailable",
            "rate limit",
            "too many requests"
        };

        var errorMessage = ex.Message.ToLower();
        return transientErrorMessages.Any(msg => errorMessage.Contains(msg)) ||
               ex is TimeoutException ||
               ex is HttpRequestException;
    }

    /// <summary>
    /// Resets the browser context to recover from authentication failures
    /// </summary>
    public async Task<bool> ResetBrowserContextForRecoveryAsync()
    {
        try
        {
            Console.WriteLine("Resetting browser context for authentication recovery...");
            
            // Clear all cookies and storage
            await Page.Context.ClearCookiesAsync();
            await Page.Context.ClearPermissionsAsync();
            
            // Clear local storage and session storage
            await Page.EvaluateAsync(@"() => {
                localStorage.clear();
                sessionStorage.clear();
            }");

            Console.WriteLine("Browser context reset completed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting browser context: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Detects if user has cancelled authentication process
    /// </summary>
    public async Task<bool> DetectAuthenticationCancellationAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            
            // Check for cancellation indicators in URL
            var cancellationIndicators = new[]
            {
                "error=access_denied",
                "error=cancelled",
                "error=user_cancelled",
                "cancelled=true",
                "auth_cancelled"
            };

            if (cancellationIndicators.Any(indicator => currentUrl.ToLower().Contains(indicator)))
            {
                Console.WriteLine($"Authentication cancellation detected in URL: {currentUrl}");
                return true;
            }

            // Check for cancellation indicators on page
            var cancellationElements = new[]
            {
                "text=cancelled",
                "text=access denied",
                "text=permission denied",
                "[data-testid='auth-cancelled']",
                "[data-testid='auth-error']"
            };

            foreach (var selector in cancellationElements)
            {
                try
                {
                    var element = await Page.WaitForSelectorAsync(selector, new() { Timeout = 1000 });
                    if (element != null)
                    {
                        Console.WriteLine($"Authentication cancellation detected via element: {selector}");
                        return true;
                    }
                }
                catch (TimeoutException)
                {
                    // Continue to next selector
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error detecting authentication cancellation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Enhanced screenshot capture with context information for debugging
    /// </summary>
    public async Task TakeDebugScreenshotAsync(string scenario, Dictionary<string, string>? context = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var filename = $"debug-{scenario}-{timestamp}";
            
            // Capture screenshot
            await TakeScreenshotAsync(filename);
            
            // Log context information
            Console.WriteLine($"Debug screenshot captured: {filename}");
            Console.WriteLine($"Current URL: {Page.Url}");
            Console.WriteLine($"Page title: {await Page.TitleAsync()}");
            
            if (context != null)
            {
                Console.WriteLine("Context information:");
                foreach (var kvp in context)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
            }

            // Capture console logs if available
            try
            {
                var logs = await Page.EvaluateAsync<string[]>(@"() => {
                    return window.console ? window.console.history || [] : [];
                }");
                
                if (logs.Length > 0)
                {
                    Console.WriteLine("Recent console logs:");
                    foreach (var log in logs.TakeLast(5))
                    {
                        Console.WriteLine($"  {log}");
                    }
                }
            }
            catch (Exception)
            {
                // Console log capture is optional
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing debug screenshot: {ex.Message}");
        }
    }

    private async Task<bool> ValidateAuthenticationState()
    {
        try
        {
            // Additional validation can be added here
            // For now, use the main IsUserAuthenticatedAsync method
            return await IsUserAuthenticatedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating authentication state: {ex.Message}");
            return false;
        }
    }
}