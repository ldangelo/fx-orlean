using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl = "https://localhost:8501")
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    public async Task NavigateAsync(string path = "/")
    {
        await Page.GotoAsync($"{BaseUrl}{path}");
        await WaitForPageLoad();
    }

    public async Task WaitForPageLoad()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task TakeScreenshotAsync(string name)
    {
        await Page.ScreenshotAsync(new()
        {
            Path = $"screenshots/{name}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png",
            FullPage = true
        });
    }

    public async Task AssertPageTitleContainsAsync(string expectedTitle)
    {
        var actualTitle = await Page.TitleAsync();
        actualTitle.Should().Contain(expectedTitle);
    }

    public async Task WaitForElementAsync(string selector, int timeoutMs = 30000)
    {
        await Page.WaitForSelectorAsync(selector, new() { Timeout = timeoutMs });
    }

    public async Task ClickAsync(string selector)
    {
        await Page.ClickAsync(selector);
    }

    public async Task FillAsync(string selector, string value)
    {
        await Page.FillAsync(selector, value);
    }

    public async Task WaitForResponseAsync(string urlPattern, Func<Task> action)
    {
        var responseTask = Page.WaitForResponseAsync(response => 
            response.Url.Contains(urlPattern) && response.Status == 200);
        
        await action();
        await responseTask;
    }

    /// <summary>
    /// Detects if user is currently authenticated by checking UI elements
    /// </summary>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public virtual async Task<bool> IsUserAuthenticatedAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            
            // If we're on a login page (contains sign-in), we're not authenticated
            var pageTitle = await Page.TitleAsync();
            if (pageTitle.ToLower().Contains("sign in") || pageTitle.ToLower().Contains("login"))
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
                "text=Sign Out",
                ".user-avatar",
                ".authenticated-header"
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
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the current page is loaded and responsive
    /// </summary>
    /// <returns>True if page is loaded</returns>
    public virtual async Task<bool> IsPageLoadedAsync()
    {
        try
        {
            // Check if page is in a loaded state
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 5000 });
            
            // Additional check for page responsiveness
            var title = await Page.TitleAsync();
            return !string.IsNullOrEmpty(title);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the given URL is an authentication-related URL
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <returns>True if URL is authentication-related</returns>
    protected virtual bool IsAuthenticationUrl(string url)
    {
        var authUrls = new[] { "/auth", "/login", "/signin", "accounts.google.com", "oauth", "keycloak" };
        return authUrls.Any(authPath => url.ToLower().Contains(authPath.ToLower()));
    }

    /// <summary>
    /// Validates that session data persists across page navigations
    /// </summary>
    /// <returns>True if session persists correctly</returns>
    public virtual async Task<bool> ValidateSessionPersistenceAsync()
    {
        try
        {
            // Store initial authentication state
            var initialAuthState = await IsUserAuthenticatedAsync();
            var initialUrl = Page.Url;
            
            Console.WriteLine($"Initial auth state: {initialAuthState}, URL: {initialUrl}");
            
            // Navigate to home and back
            await NavigateAsync("/");
            await Task.Delay(1000); // Allow page to stabilize
            
            var homeAuthState = await IsUserAuthenticatedAsync();
            Console.WriteLine($"Home page auth state: {homeAuthState}");
            
            // Navigate back to original page (or a test page)
            if (initialUrl != Page.Url)
            {
                await Page.GotoAsync(initialUrl);
                await Task.Delay(1000);
            }
            
            var finalAuthState = await IsUserAuthenticatedAsync();
            Console.WriteLine($"Final auth state: {finalAuthState}");
            
            // Session should persist across navigations
            return initialAuthState == homeAuthState && homeAuthState == finalAuthState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session persistence validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validates that authentication cookies/tokens are maintained
    /// </summary>
    /// <returns>True if authentication cookies persist</returns>
    public virtual async Task<bool> ValidateAuthenticationCookiesAsync()
    {
        try
        {
            // Get all cookies
            var cookies = await Page.Context.CookiesAsync();
            
            // Look for authentication-related cookies
            var authCookies = cookies.Where(c => 
                c.Name.ToLower().Contains("auth") || 
                c.Name.ToLower().Contains("session") ||
                c.Name.ToLower().Contains("token") ||
                c.Name.ToLower().Contains("identity") ||
                c.Name.ToLower().Contains("keycloak")).ToArray();
            
            Console.WriteLine($"Found {authCookies.Length} authentication-related cookies");
            
            foreach (var cookie in authCookies)
            {
                Console.WriteLine($"Auth cookie: {cookie.Name} = {cookie.Value[..Math.Min(cookie.Value.Length, 20)]}...");
            }
            
            // Should have at least one authentication cookie if user is authenticated
            var isAuthenticated = await IsUserAuthenticatedAsync();
            return !isAuthenticated || authCookies.Length > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cookie validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validates that user context information is available across page objects
    /// </summary>
    /// <returns>True if user context is accessible</returns>
    public virtual async Task<bool> ValidateUserContextAvailabilityAsync()
    {
        try
        {
            var isAuthenticated = await IsUserAuthenticatedAsync();
            
            if (!isAuthenticated)
            {
                Console.WriteLine("User not authenticated - user context validation skipped");
                return true; // Valid state for non-authenticated users
            }
            
            // Check if user information is available in page
            var userInfoElements = new[]
            {
                "[data-testid='user-name']",
                "[data-testid='user-email']",
                "[data-testid='user-menu']",
                "[data-testid='user-avatar']",
                ".user-info",
                ".current-user"
            };
            
            var userInfoFound = false;
            foreach (var selector in userInfoElements)
            {
                try
                {
                    var element = await Page.WaitForSelectorAsync(selector, new() { Timeout = 2000 });
                    if (element != null)
                    {
                        var text = await element.TextContentAsync();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            Console.WriteLine($"Found user info: {selector} = {text}");
                            userInfoFound = true;
                            break;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Continue to next selector
                }
            }
            
            return userInfoFound;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"User context validation error: {ex.Message}");
            return false;
        }
    }
}