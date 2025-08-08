using Microsoft.Playwright;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Services;

/// <summary>
/// Service for managing browser-specific configurations and optimizations
/// </summary>
public class BrowserConfigurationService
{
    private readonly Dictionary<string, BrowserConfiguration> _browserConfigs;

    public BrowserConfigurationService()
    {
        _browserConfigs = new Dictionary<string, BrowserConfiguration>
        {
            ["chromium"] = new BrowserConfiguration
            {
                Name = "Chromium",
                DefaultTimeout = 60000,
                AuthenticationTimeout = 90000, // Chromium tends to be faster with OAuth
                RetryMultiplier = 1.0,
                SupportsWebKit = false,
                RequiresSpecialHandling = false,
                OptimalViewportSize = new ViewportSize(1280, 720),
                Args = new[]
                {
                    "--disable-web-security",
                    "--disable-blink-features=AutomationControlled",
                    "--disable-extensions"
                }
            },
            
            ["firefox"] = new BrowserConfiguration
            {
                Name = "Firefox",
                DefaultTimeout = 75000, // Firefox can be slower
                AuthenticationTimeout = 120000,
                RetryMultiplier = 1.5, // More retries for Firefox
                SupportsWebKit = false,
                RequiresSpecialHandling = true,
                OptimalViewportSize = new ViewportSize(1280, 720),
                Args = new[]
                {
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows"
                }
            },
            
            ["webkit"] = new BrowserConfiguration
            {
                Name = "WebKit",
                DefaultTimeout = 90000, // WebKit can be variable
                AuthenticationTimeout = 120000,
                RetryMultiplier = 2.0, // WebKit may need more retries
                SupportsWebKit = true,
                RequiresSpecialHandling = true,
                OptimalViewportSize = new ViewportSize(1280, 720),
                Args = Array.Empty<string>() // WebKit has fewer configuration options
            }
        };
    }

    /// <summary>
    /// Gets configuration for a specific browser
    /// </summary>
    public BrowserConfiguration GetConfiguration(string browserName)
    {
        var normalizedName = browserName.ToLower();
        
        if (_browserConfigs.TryGetValue(normalizedName, out var config))
        {
            return config;
        }

        // Default configuration for unknown browsers
        return new BrowserConfiguration
        {
            Name = browserName,
            DefaultTimeout = 60000,
            AuthenticationTimeout = 90000,
            RetryMultiplier = 1.0,
            SupportsWebKit = false,
            RequiresSpecialHandling = false,
            OptimalViewportSize = new ViewportSize(1280, 720),
            Args = Array.Empty<string>()
        };
    }

    /// <summary>
    /// Gets OAuth-specific timeout for a browser
    /// </summary>
    public int GetOAuthTimeout(string browserName, int baseTimeout = 60000)
    {
        var config = GetConfiguration(browserName);
        return Math.Max(baseTimeout, config.AuthenticationTimeout);
    }

    /// <summary>
    /// Gets retry attempts adjusted for browser characteristics
    /// </summary>
    public int GetRetryAttempts(string browserName, int baseRetryAttempts = 3)
    {
        var config = GetConfiguration(browserName);
        return (int)Math.Ceiling(baseRetryAttempts * config.RetryMultiplier);
    }

    /// <summary>
    /// Determines if a browser requires special handling for OAuth
    /// </summary>
    public bool RequiresSpecialOAuthHandling(string browserName)
    {
        var config = GetConfiguration(browserName);
        return config.RequiresSpecialHandling;
    }

    /// <summary>
    /// Gets optimal viewport size for a browser
    /// </summary>
    public ViewportSize GetOptimalViewportSize(string browserName)
    {
        var config = GetConfiguration(browserName);
        return config.OptimalViewportSize;
    }

    /// <summary>
    /// Gets browser launch arguments
    /// </summary>
    public string[] GetLaunchArgs(string browserName)
    {
        var config = GetConfiguration(browserName);
        return config.Args;
    }

    /// <summary>
    /// Creates browser launch options optimized for authentication testing
    /// </summary>
    public BrowserTypeLaunchOptions CreateLaunchOptions(string browserName, bool headless = false)
    {
        var config = GetConfiguration(browserName);
        
        // Check environment variable for headless mode override
        var envHeadless = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS");
        var isHeadless = headless;
        
        if (envHeadless != null)
        {
            isHeadless = bool.Parse(envHeadless);
        }
        
        // For OAuth testing, default to headed mode unless explicitly set to headless
        if (!isHeadless)
        {
            Console.WriteLine($"🔍 Running {browserName} in HEADED mode for OAuth authentication");
        }
        
        var options = new BrowserTypeLaunchOptions
        {
            Headless = isHeadless,
            Args = config.Args,
            SlowMo = config.RequiresSpecialHandling ? 100 : 50, // Always add some delay for OAuth flows
            Timeout = config.DefaultTimeout
        };

        return options;
    }

    /// <summary>
    /// Creates browser context options optimized for authentication
    /// </summary>
    public BrowserNewContextOptions CreateContextOptions(string browserName)
    {
        var config = GetConfiguration(browserName);
        
        var options = new BrowserNewContextOptions
        {
            ViewportSize = new()
            {
                Width = config.OptimalViewportSize.Width,
                Height = config.OptimalViewportSize.Height
            },
            IgnoreHTTPSErrors = true, // Useful for test environments
            AcceptDownloads = false, // Not needed for OAuth testing
            JavaScriptEnabled = true,
            Permissions = new[] { "geolocation" }, // Some OAuth flows may request location
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Accept-Language"] = "en-US,en;q=0.9"
            }
        };

        // Browser-specific context adjustments
        switch (browserName.ToLower())
        {
            case "firefox":
                // Firefox may need additional permissions
                options.Permissions = new[] { "geolocation", "notifications" };
                break;
                
            case "webkit":
                // WebKit is more restrictive, minimal permissions
                options.Permissions = Array.Empty<string>();
                break;
        }

        return options;
    }

    /// <summary>
    /// Gets all supported browsers for testing
    /// </summary>
    public IEnumerable<string> GetSupportedBrowsers()
    {
        return _browserConfigs.Keys.Select(k => _browserConfigs[k].Name);
    }

    /// <summary>
    /// Validates if browser is available for testing
    /// </summary>
    public async Task<bool> IsBrowserAvailableAsync(string browserName)
    {
        try
        {
            var playwright = await Playwright.CreateAsync();
            
            return browserName.ToLower() switch
            {
                "chromium" => await TryLaunchBrowser(playwright.Chromium, browserName),
                "firefox" => await TryLaunchBrowser(playwright.Firefox, browserName),
                "webkit" => await TryLaunchBrowser(playwright.Webkit, browserName),
                _ => false
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Browser availability check failed for {browserName}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TryLaunchBrowser(IBrowserType browserType, string browserName)
    {
        try
        {
            var options = CreateLaunchOptions(browserName, headless: true);
            var browser = await browserType.LaunchAsync(options);
            await browser.CloseAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets performance expectations for a browser
    /// </summary>
    public BrowserPerformanceProfile GetPerformanceProfile(string browserName)
    {
        var config = GetConfiguration(browserName);
        
        return new BrowserPerformanceProfile
        {
            BrowserName = browserName,
            ExpectedStartupTime = config.RequiresSpecialHandling ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(2),
            ExpectedNavigationTime = config.RequiresSpecialHandling ? TimeSpan.FromSeconds(3) : TimeSpan.FromSeconds(1),
            ExpectedOAuthFlowTime = TimeSpan.FromMilliseconds(config.AuthenticationTimeout),
            ReliabilityScore = config.RequiresSpecialHandling ? 0.8 : 0.95, // Lower reliability for problematic browsers
            RecommendedConcurrency = config.RequiresSpecialHandling ? 1 : 3 // Fewer concurrent instances for problematic browsers
        };
    }
}

/// <summary>
/// Browser-specific configuration settings
/// </summary>
public class BrowserConfiguration
{
    public string Name { get; set; } = string.Empty;
    public int DefaultTimeout { get; set; }
    public int AuthenticationTimeout { get; set; }
    public double RetryMultiplier { get; set; }
    public bool SupportsWebKit { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public ViewportSize OptimalViewportSize { get; set; } = new(1280, 720);
    public string[] Args { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Viewport size configuration
/// </summary>
public class ViewportSize
{
    public int Width { get; set; }
    public int Height { get; set; }

    public ViewportSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}

/// <summary>
/// Browser performance characteristics
/// </summary>
public class BrowserPerformanceProfile
{
    public string BrowserName { get; set; } = string.Empty;
    public TimeSpan ExpectedStartupTime { get; set; }
    public TimeSpan ExpectedNavigationTime { get; set; }
    public TimeSpan ExpectedOAuthFlowTime { get; set; }
    public double ReliabilityScore { get; set; } // 0.0 to 1.0
    public int RecommendedConcurrency { get; set; }
}

/// <summary>
/// Cross-browser test results aggregation
/// </summary>
public class CrossBrowserTestResults
{
    public Dictionary<string, bool> BrowserResults { get; set; } = new();
    public Dictionary<string, TimeSpan> PerformanceResults { get; set; } = new();
    public Dictionary<string, Exception?> Errors { get; set; } = new();
    public DateTime TestStartTime { get; set; }
    public DateTime TestEndTime { get; set; }
    
    public double SuccessRate => BrowserResults.Count > 0 ? BrowserResults.Values.Count(v => v) / (double)BrowserResults.Count : 0;
    public TimeSpan TotalDuration => TestEndTime - TestStartTime;
    public TimeSpan AveragePerformance => PerformanceResults.Count > 0 
        ? TimeSpan.FromTicks((long)PerformanceResults.Values.Average(t => t.Ticks)) 
        : TimeSpan.Zero;
    
    public override string ToString()
    {
        return $"Cross-Browser Results: {SuccessRate:P1} success rate, " +
               $"{BrowserResults.Count} browsers tested, " +
               $"Average time: {AveragePerformance.TotalSeconds:F2}s";
    }
}