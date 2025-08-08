namespace FxExpert.E2E.Tests.Configuration;

/// <summary>
/// Configuration settings for E2E test authentication
/// </summary>
public class AuthenticationConfiguration
{
    /// <summary>
    /// Authentication mode for E2E tests
    /// </summary>
    public AuthenticationMode Mode { get; set; } = AuthenticationMode.Manual;

    /// <summary>
    /// Timeout in milliseconds for OAuth flow completion
    /// </summary>
    public int Timeout { get; set; } = 60000;

    /// <summary>
    /// Number of retry attempts for failed authentication
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Test account credentials (only used in Automated mode)
    /// </summary>
    public TestAccountCredentials? TestAccount { get; set; }

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    public bool IsValid()
    {
        // Timeout must be positive
        if (Timeout <= 0)
            return false;

        // Retry attempts must be non-negative
        if (RetryAttempts < 0)
            return false;

        // If automated mode, test account credentials are required
        if (Mode == AuthenticationMode.Automated && TestAccount == null)
            return false;

        // If automated mode with credentials, validate they're not empty
        if (Mode == AuthenticationMode.Automated && TestAccount != null)
        {
            if (string.IsNullOrWhiteSpace(TestAccount.Email) || 
                string.IsNullOrWhiteSpace(TestAccount.Password))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Authentication modes for E2E testing
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Manual authentication - wait for user to complete OAuth flow
    /// </summary>
    Manual,

    /// <summary>
    /// Automated authentication - use test credentials to complete OAuth flow
    /// </summary>
    Automated
}

/// <summary>
/// Test account credentials for automated authentication
/// </summary>
public class TestAccountCredentials
{
    /// <summary>
    /// Google account email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Google account password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Validates that credentials are not empty
    /// </summary>
    /// <returns>True if credentials are valid</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password);
    }
}

/// <summary>
/// Environment-specific configuration profile
/// </summary>
public class EnvironmentProfile
{
    /// <summary>
    /// Profile name (Development, CI, Local, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether to run browsers in headless mode
    /// </summary>
    public bool HeadlessMode { get; set; } = false;

    /// <summary>
    /// Browser timeout adjustments for environment
    /// </summary>
    public int BrowserTimeoutMultiplier { get; set; } = 1;

    /// <summary>
    /// Whether to capture screenshots on failure
    /// </summary>
    public bool CaptureScreenshots { get; set; } = true;

    /// <summary>
    /// Whether to capture videos of test execution
    /// </summary>
    public bool CaptureVideos { get; set; } = false;

    /// <summary>
    /// Default configuration for Development environment
    /// </summary>
    public static EnvironmentProfile Development => new()
    {
        Name = "Development",
        HeadlessMode = false,
        BrowserTimeoutMultiplier = 1,
        CaptureScreenshots = true,
        CaptureVideos = false
    };

    /// <summary>
    /// Default configuration for CI environment
    /// </summary>
    public static EnvironmentProfile CI => new()
    {
        Name = "CI",
        HeadlessMode = true,
        BrowserTimeoutMultiplier = 2, // Longer timeouts for CI
        CaptureScreenshots = true,
        CaptureVideos = true // Capture videos for CI debugging
    };

    /// <summary>
    /// Default configuration for Local environment
    /// </summary>
    public static EnvironmentProfile Local => new()
    {
        Name = "Local",
        HeadlessMode = false,
        BrowserTimeoutMultiplier = 1,
        CaptureScreenshots = false, // Less noise for local dev
        CaptureVideos = false
    };
}