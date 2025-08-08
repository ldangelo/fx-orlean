using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class AuthenticationConfigurationTests
{
    private IConfiguration _configuration;
    private AuthenticationConfigurationManager _configManager;

    [SetUp]
    public void SetUp()
    {
        // Create test configuration with in-memory provider
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Manual",
                ["Authentication:Timeout"] = "60000",
                ["Authentication:RetryAttempts"] = "3",
                ["Authentication:TestAccount:Email"] = "test@example.com",
                ["Authentication:TestAccount:Password"] = "test-password"
            });

        _configuration = configBuilder.Build();
        _configManager = new AuthenticationConfigurationManager(_configuration);
    }

    [Test]
    [Category("Unit")]
    public async Task LoadAuthenticationConfig_WithValidConfiguration_ShouldReturnConfiguration()
    {
        // Act
        var config = await _configManager.LoadAuthenticationConfigAsync();

        // Assert
        config.Should().NotBeNull("Configuration should be loaded successfully");
        config.Mode.Should().Be(AuthenticationMode.Manual, "Mode should match configuration value");
        config.Timeout.Should().Be(60000, "Timeout should match configuration value");
        config.RetryAttempts.Should().Be(3, "RetryAttempts should match configuration value");
    }

    [Test]
    [Category("Unit")]
    public async Task LoadAuthenticationConfig_WithMissingConfiguration_ShouldUseDefaults()
    {
        // Arrange - Create configuration with missing values
        var emptyConfigBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>());
        var emptyConfig = emptyConfigBuilder.Build();
        var emptyConfigManager = new AuthenticationConfigurationManager(emptyConfig);

        // Act
        var config = await emptyConfigManager.LoadAuthenticationConfigAsync();

        // Assert
        config.Should().NotBeNull("Configuration should still be created with defaults");
        config.Mode.Should().Be(AuthenticationMode.Manual, "Default mode should be Manual");
        config.Timeout.Should().Be(60000, "Default timeout should be 60 seconds");
        config.RetryAttempts.Should().Be(3, "Default retry attempts should be 3");
    }

    [Test]
    [Category("Unit")]
    public async Task ValidateConfiguration_WithValidConfig_ShouldReturnTrue()
    {
        // Arrange
        var config = await _configManager.LoadAuthenticationConfigAsync();

        // Act
        var isValid = await _configManager.ValidateConfigurationAsync(config);

        // Assert
        isValid.Should().BeTrue("Valid configuration should pass validation");
    }

    [Test]
    [Category("Unit")]
    public async Task ValidateConfiguration_WithInvalidTimeout_ShouldReturnFalse()
    {
        // Arrange
        var invalidConfigBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Manual",
                ["Authentication:Timeout"] = "-1000", // Invalid negative timeout
                ["Authentication:RetryAttempts"] = "3"
            });

        var invalidConfig = invalidConfigBuilder.Build();
        var invalidConfigManager = new AuthenticationConfigurationManager(invalidConfig);
        var config = await invalidConfigManager.LoadAuthenticationConfigAsync();

        // Act
        var isValid = await invalidConfigManager.ValidateConfigurationAsync(config);

        // Assert
        isValid.Should().BeFalse("Invalid timeout should fail validation");
    }

    [Test]
    [Category("Unit")]
    public async Task LoadTestCredentials_WithAutomatedMode_ShouldReturnCredentials()
    {
        // Arrange - Set up configuration for automated mode
        var automatedConfigBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Automated",
                ["Authentication:TestAccount:Email"] = "test@example.com",
                ["Authentication:TestAccount:Password"] = "test-password"
            });

        var automatedConfig = automatedConfigBuilder.Build();
        var automatedConfigManager = new AuthenticationConfigurationManager(automatedConfig);

        // Act
        var credentials = await automatedConfigManager.LoadTestCredentialsAsync();

        // Assert
        credentials.Should().NotBeNull("Credentials should be loaded for automated mode");
        credentials.Email.Should().Be("test@example.com", "Email should match configuration");
        credentials.Password.Should().Be("test-password", "Password should match configuration");
    }

    [Test]
    [Category("Unit")]
    public async Task LoadTestCredentials_WithManualMode_ShouldReturnNull()
    {
        // Act - Default configuration is Manual mode
        var credentials = await _configManager.LoadTestCredentialsAsync();

        // Assert
        credentials.Should().BeNull("Manual mode should not return credentials");
    }

    [Test]
    [Category("Unit")]
    public async Task GetEnvironmentProfile_WithDevelopmentEnvironment_ShouldReturnDevelopmentConfig()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        // Act
        var profile = await _configManager.GetEnvironmentProfileAsync();

        // Assert
        profile.Should().NotBeNull("Environment profile should be loaded");
        profile.Name.Should().Be("Development", "Profile name should match environment");
        profile.HeadlessMode.Should().BeFalse("Development should default to headed mode");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Test]
    [Category("Unit")]
    public async Task GetEnvironmentProfile_WithCIEnvironment_ShouldReturnCIConfig()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "CI");

        // Act
        var profile = await _configManager.GetEnvironmentProfileAsync();

        // Assert
        profile.Should().NotBeNull("CI profile should be loaded");
        profile.Name.Should().Be("CI", "Profile name should match environment");
        profile.HeadlessMode.Should().BeTrue("CI should default to headless mode");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Test]
    [Category("Unit")]
    public async Task LoadConfiguration_WithUserSecrets_ShouldOverrideDefaults()
    {
        // Arrange - Simulate user secrets configuration
        var userSecretsBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Automated",
                ["Authentication:Timeout"] = "45000",
                ["Authentication:TestAccount:Email"] = "secrets@example.com"
            });

        var userSecretsConfig = userSecretsBuilder.Build();
        var userSecretsManager = new AuthenticationConfigurationManager(userSecretsConfig);

        // Act
        var config = await userSecretsManager.LoadAuthenticationConfigAsync();

        // Assert
        config.Mode.Should().Be(AuthenticationMode.Automated, "User secrets should override default mode");
        config.Timeout.Should().Be(45000, "User secrets should override default timeout");
    }

    [Test]
    [Category("Unit")]
    public async Task LoadConfiguration_WithEnvironmentVariables_ShouldOverrideConfig()
    {
        // Arrange - Set environment variables
        Environment.SetEnvironmentVariable("Authentication__Mode", "Automated");
        Environment.SetEnvironmentVariable("Authentication__Timeout", "30000");

        var envConfigBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Mode"] = "Manual",
                ["Authentication:Timeout"] = "60000"
            })
            .AddEnvironmentVariables();

        var envConfig = envConfigBuilder.Build();
        var envConfigManager = new AuthenticationConfigurationManager(envConfig);

        // Act
        var config = await envConfigManager.LoadAuthenticationConfigAsync();

        // Assert
        config.Mode.Should().Be(AuthenticationMode.Automated, "Environment variables should override config");
        config.Timeout.Should().Be(30000, "Environment variables should override config timeout");

        // Cleanup
        Environment.SetEnvironmentVariable("Authentication__Mode", null);
        Environment.SetEnvironmentVariable("Authentication__Timeout", null);
    }

    [Test]
    [Category("Unit")]
    public async Task ConfigurationPriority_EnvironmentVariables_ShouldHaveHighestPriority()
    {
        // Arrange - Set up configuration with multiple sources (environment wins)
        Environment.SetEnvironmentVariable("Authentication__RetryAttempts", "5");

        var multiSourceBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:RetryAttempts"] = "2" // Lower priority
            })
            .AddEnvironmentVariables(); // Higher priority

        var multiSourceConfig = multiSourceBuilder.Build();
        var multiSourceManager = new AuthenticationConfigurationManager(multiSourceConfig);

        // Act
        var config = await multiSourceManager.LoadAuthenticationConfigAsync();

        // Assert
        config.RetryAttempts.Should().Be(5, "Environment variables should have highest priority");

        // Cleanup
        Environment.SetEnvironmentVariable("Authentication__RetryAttempts", null);
    }

    [Test]
    [Category("Unit")]
    public async Task AuthenticationConfigurationManager_ShouldInitializeWithConfiguration()
    {
        // Act & Assert
        await Task.CompletedTask;
        var configManager = new AuthenticationConfigurationManager(_configuration);
        configManager.Should().NotBeNull("AuthenticationConfigurationManager should initialize with configuration");
    }
}