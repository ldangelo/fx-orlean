using Microsoft.Extensions.Configuration;

namespace FxExpert.E2E.Tests.Configuration;

/// <summary>
/// Manages authentication configuration loading and validation for E2E tests
/// </summary>
public class AuthenticationConfigurationManager
{
    private readonly IConfiguration _configuration;
    private const string AUTHENTICATION_SECTION = "Authentication";

    /// <summary>
    /// Initializes the configuration manager with the provided configuration
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    public AuthenticationConfigurationManager(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Loads authentication configuration with defaults and validation
    /// </summary>
    /// <returns>Authentication configuration</returns>
    public async Task<AuthenticationConfiguration> LoadAuthenticationConfigAsync()
    {
        try
        {
            var config = new AuthenticationConfiguration();

            // Load configuration from various sources (appsettings, user secrets, environment variables)
            var authSection = _configuration.GetSection(AUTHENTICATION_SECTION);

            // Load authentication mode
            if (Enum.TryParse<AuthenticationMode>(authSection["Mode"], out var mode))
            {
                config.Mode = mode;
            }

            // Load timeout (validation done later in IsValid())
            if (int.TryParse(authSection["Timeout"], out var timeout))
            {
                config.Timeout = timeout;
            }

            // Load retry attempts (validation done later in IsValid())
            if (int.TryParse(authSection["RetryAttempts"], out var retryAttempts))
            {
                config.RetryAttempts = retryAttempts;
            }

            // Load test account credentials if in automated mode
            if (config.Mode == AuthenticationMode.Automated)
            {
                var testAccountSection = authSection.GetSection("TestAccount");
                var email = testAccountSection["Email"];
                var password = testAccountSection["Password"];

                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                {
                    config.TestAccount = new TestAccountCredentials
                    {
                        Email = email,
                        Password = password
                    };
                }
            }

            return await Task.FromResult(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading authentication configuration: {ex.Message}");
            // Return default configuration on error
            return await Task.FromResult(new AuthenticationConfiguration());
        }
    }

    /// <summary>
    /// Validates the authentication configuration
    /// </summary>
    /// <param name="config">Configuration to validate</param>
    /// <returns>True if configuration is valid</returns>
    public async Task<bool> ValidateConfigurationAsync(AuthenticationConfiguration config)
    {
        try
        {
            if (config == null)
            {
                Console.WriteLine("Authentication configuration is null");
                return false;
            }

            var isValid = config.IsValid();
            
            if (!isValid)
            {
                Console.WriteLine("Authentication configuration validation failed");
            }

            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating authentication configuration: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    /// Loads test credentials for automated authentication
    /// </summary>
    /// <returns>Test credentials or null if manual mode</returns>
    public async Task<TestAccountCredentials?> LoadTestCredentialsAsync()
    {
        try
        {
            var config = await LoadAuthenticationConfigAsync();

            // Only return credentials for automated mode
            if (config.Mode == AuthenticationMode.Automated && config.TestAccount != null)
            {
                return config.TestAccount;
            }

            return await Task.FromResult<TestAccountCredentials?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading test credentials: {ex.Message}");
            return await Task.FromResult<TestAccountCredentials?>(null);
        }
    }

    /// <summary>
    /// Gets environment-specific configuration profile
    /// </summary>
    /// <returns>Environment profile configuration</returns>
    public async Task<EnvironmentProfile> GetEnvironmentProfileAsync()
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var profile = environment.ToUpper() switch
            {
                "DEVELOPMENT" => EnvironmentProfile.Development,
                "CI" => EnvironmentProfile.CI,
                "LOCAL" => EnvironmentProfile.Local,
                _ => EnvironmentProfile.Local
            };

            // Allow configuration overrides for environment profiles
            var profileSection = _configuration.GetSection($"EnvironmentProfiles:{environment}");
            
            if (profileSection.Exists())
            {
                // Override headless mode if specified
                if (bool.TryParse(profileSection["HeadlessMode"], out var headless))
                {
                    profile.HeadlessMode = headless;
                }

                // Override timeout multiplier if specified
                if (int.TryParse(profileSection["BrowserTimeoutMultiplier"], out var timeoutMultiplier) && timeoutMultiplier > 0)
                {
                    profile.BrowserTimeoutMultiplier = timeoutMultiplier;
                }

                // Override screenshot capture if specified
                if (bool.TryParse(profileSection["CaptureScreenshots"], out var screenshots))
                {
                    profile.CaptureScreenshots = screenshots;
                }

                // Override video capture if specified
                if (bool.TryParse(profileSection["CaptureVideos"], out var videos))
                {
                    profile.CaptureVideos = videos;
                }
            }

            return await Task.FromResult(profile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting environment profile: {ex.Message}");
            return await Task.FromResult(EnvironmentProfile.Local);
        }
    }

    /// <summary>
    /// Creates a configuration manager with default configuration sources
    /// </summary>
    /// <param name="environment">Environment name (Development, CI, Local)</param>
    /// <returns>Configuration manager with proper source hierarchy</returns>
    public static AuthenticationConfigurationManager CreateDefault(string environment = "Local")
    {
        var builder = new ConfigurationBuilder();

        // Add configuration sources in priority order (last wins)
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

        // Add user secrets for Development environment
        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            // Note: User Secrets require a UserSecretsId in the project file
            // builder.AddUserSecrets<AuthenticationConfigurationManager>();
        }

        // Environment variables have highest priority
        builder.AddEnvironmentVariables();

        var configuration = builder.Build();
        return new AuthenticationConfigurationManager(configuration);
    }

    /// <summary>
    /// Gets the effective timeout for authentication operations based on environment
    /// </summary>
    /// <returns>Effective timeout in milliseconds</returns>
    public async Task<int> GetEffectiveTimeoutAsync()
    {
        try
        {
            var config = await LoadAuthenticationConfigAsync();
            var profile = await GetEnvironmentProfileAsync();

            return config.Timeout * profile.BrowserTimeoutMultiplier;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating effective timeout: {ex.Message}");
            return 60000; // Default 60 seconds
        }
    }
}