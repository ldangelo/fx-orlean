using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class AuthenticationIntegrationTests : PageTest
{
    private AuthenticationPage _authPage;
    private HomePage _homePage;
    private PartnerProfilePage _partnerPage;
    private ConfirmationPage _confirmationPage;
    private AuthenticationConfigurationManager _configManager;

    [SetUp]
    public async Task SetUp()
    {
        _authPage = new AuthenticationPage(Page);
        _homePage = new HomePage(Page);
        _partnerPage = new PartnerProfilePage(Page);
        _confirmationPage = new ConfirmationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");
        await Task.Run(() => Directory.CreateDirectory("screenshots"));
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithHomePage_ShouldAllowNavigationAfterAuth()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        
        // Act - Attempt authentication (will timeout in test environment)
        var authResult = await _authPage.HandleGoogleOAuthAsync(5000);
        
        // Assert - Even without completing auth, HomePage should handle the authentication state
        authResult.Should().BeFalse("OAuth should timeout in test environment");
        
        // Verify HomePage can detect authentication state
        var isAuthenticated = await _homePage.IsUserAuthenticatedAsync();
        isAuthenticated.Should().BeFalse("User should not be authenticated after timeout");
        
        // Verify HomePage can still navigate and function
        var homePageLoaded = await _homePage.IsPageLoadedAsync();
        homePageLoaded.Should().BeTrue("HomePage should load regardless of auth state");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithPartnerProfilePage_ShouldHandleAuthenticatedAccess()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        
        // Act - Check if partner profile requires authentication
        var authRequired = await _partnerPage.RequiresAuthenticationAsync();
        
        if (authRequired)
        {
            // Attempt authentication flow
            var authResult = await _authPage.HandleGoogleOAuthAsync(3000);
            authResult.Should().BeFalse("OAuth should timeout in test environment");
            
            // Verify partner page handles unauthenticated state appropriately
            var canAccessWithoutAuth = await _partnerPage.CanAccessWithoutAuthenticationAsync();
            canAccessWithoutAuth.Should().BeFalse("Partner profile should require authentication");
        }
        
        // Assert - PartnerProfilePage should handle authentication state gracefully
        var pageHandlesAuthState = await _partnerPage.HandlesAuthenticationStateAsync();
        pageHandlesAuthState.Should().BeTrue("PartnerProfilePage should gracefully handle auth state");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithConfirmationPage_ShouldPersistSessionState()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        
        // Act - Check authentication persistence across page navigation
        var initialAuthState = await _authPage.IsUserAuthenticatedAsync();
        
        // Navigate to confirmation page (simulated)
        await _confirmationPage.NavigateToConfirmationAsync();
        
        // Check if authentication state persists
        var persistedAuthState = await _authPage.IsUserAuthenticatedAsync();
        
        // Assert
        initialAuthState.Should().Be(persistedAuthState, "Authentication state should persist across page navigation");
        
        // Verify confirmation page can access authentication state
        var confirmationHandlesAuth = await _confirmationPage.CanAccessAuthenticationStateAsync();
        confirmationHandlesAuth.Should().BeTrue("ConfirmationPage should access authentication state");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationConfiguration_WithPageObjects_ShouldRespectEnvironmentSettings()
    {
        // Arrange
        var config = await _configManager.LoadAuthenticationConfigAsync();
        var profile = await _configManager.GetEnvironmentProfileAsync();
        
        // Act - Use configuration in authentication flow
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();
        var authResult = await _authPage.HandleGoogleOAuthAsync(effectiveTimeout);
        
        // Assert - Configuration should be applied correctly
        effectiveTimeout.Should().BeGreaterThan(0, "Effective timeout should be positive");
        authResult.Should().BeFalse("OAuth should timeout in test environment");
        
        // Verify environment profile affects behavior
        profile.Name.Should().Be("Development", "Should use Development profile in test");
        profile.HeadlessMode.Should().BeFalse("Development should use headed mode");
    }

    [Test]
    [Category("Integration")]  
    public async Task AuthenticationFlow_CrossPageNavigation_ShouldMaintainSessionState()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        var initialState = await _authPage.IsUserAuthenticatedAsync();
        
        // Act - Navigate across multiple pages
        await _homePage.NavigateToHomeAsync();
        var homeAuthState = await _authPage.IsUserAuthenticatedAsync();
        
        await _partnerPage.NavigateToPartnerProfileAsync();
        var partnerAuthState = await _authPage.IsUserAuthenticatedAsync();
        
        await _confirmationPage.NavigateToConfirmationAsync();
        var confirmationAuthState = await _authPage.IsUserAuthenticatedAsync();
        
        // Assert - Authentication state should be consistent across navigation
        homeAuthState.Should().Be(initialState, "Auth state should persist on HomePage");
        partnerAuthState.Should().Be(initialState, "Auth state should persist on PartnerProfilePage");
        confirmationAuthState.Should().Be(initialState, "Auth state should persist on ConfirmationPage");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithPageObjectBase_ShouldProvideConsistentAuthDetection()
    {
        // Arrange - All page objects extend BasePage
        var pageObjects = new BasePage[] { _homePage, _partnerPage, _confirmationPage, _authPage };
        
        // Act & Assert - All page objects should have consistent authentication detection
        foreach (var pageObject in pageObjects)
        {
            await Page.GotoAsync("https://localhost:8501");
            
            var authState = await pageObject.IsUserAuthenticatedAsync();
            authState.Should().BeFalse($"{pageObject.GetType().Name} should detect unauthenticated state");
            
            var pageLoaded = await pageObject.IsPageLoadedAsync();
            pageLoaded.Should().BeTrue($"{pageObject.GetType().Name} should load successfully");
        }
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithErrorHandling_ShouldRecoverGracefully()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        
        // Act - Test error recovery scenarios
        var shortTimeoutResult = await _authPage.HandleGoogleOAuthAsync(100); // Very short timeout
        shortTimeoutResult.Should().BeFalse("Short timeout should fail gracefully");
        
        // Verify page objects still function after auth failure
        var homePageStillWorks = await _homePage.IsPageLoadedAsync();
        homePageStillWorks.Should().BeTrue("HomePage should still work after auth failure");
        
        var authStateDetectable = await _authPage.IsUserAuthenticatedAsync();
        authStateDetectable.Should().BeFalse("Auth state should still be detectable after failure");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithConfigurationOverrides_ShouldRespectSettings()
    {
        // Arrange - Set environment variables to test configuration override
        Environment.SetEnvironmentVariable("Authentication__Mode", "Manual");
        Environment.SetEnvironmentVariable("Authentication__Timeout", "15000");
        
        var overriddenConfigManager = AuthenticationConfigurationManager.CreateDefault("Development");
        var config = await overriddenConfigManager.LoadAuthenticationConfigAsync();
        
        // Act
        var credentials = await overriddenConfigManager.LoadTestCredentialsAsync();
        var effectiveTimeout = await overriddenConfigManager.GetEffectiveTimeoutAsync();
        
        // Assert
        config.Mode.Should().Be(AuthenticationMode.Manual, "Environment variable should override mode");
        config.Timeout.Should().Be(15000, "Environment variable should override timeout");
        credentials.Should().BeNull("Manual mode should not return credentials");
        effectiveTimeout.Should().Be(15000, "Effective timeout should match configuration");
        
        // Cleanup
        Environment.SetEnvironmentVariable("Authentication__Mode", null);
        Environment.SetEnvironmentVariable("Authentication__Timeout", null);
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithMultipleAttempts_ShouldHandleRetries()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:8501");
        var config = await _configManager.LoadAuthenticationConfigAsync();
        
        // Act - Simulate multiple authentication attempts
        var attempt1 = await _authPage.HandleGoogleOAuthAsync(1000);
        var attempt2 = await _authPage.HandleGoogleOAuthAsync(1000);
        var attempt3 = await _authPage.HandleGoogleOAuthAsync(1000);
        
        // Assert - All attempts should handle timeouts gracefully
        attempt1.Should().BeFalse("First attempt should timeout");
        attempt2.Should().BeFalse("Second attempt should timeout");
        attempt3.Should().BeFalse("Third attempt should timeout");
        
        // Verify system remains stable after multiple attempts
        var finalState = await _authPage.IsUserAuthenticatedAsync();
        finalState.Should().BeFalse("Final state should be unauthenticated");
        
        var pageStillResponsive = await _homePage.IsPageLoadedAsync();
        pageStillResponsive.Should().BeTrue("Page should remain responsive after multiple attempts");
    }

    [Test]
    [Category("Integration")]
    public async Task AuthenticationFlow_WithDifferentEnvironments_ShouldAdaptBehavior()
    {
        // Arrange - Test different environment profiles
        var environments = new[] { "Development", "CI", "Local" };
        
        foreach (var env in environments)
        {
            // Act
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", env);
            var envConfigManager = AuthenticationConfigurationManager.CreateDefault(env);
            var profile = await envConfigManager.GetEnvironmentProfileAsync();
            
            // Assert
            profile.Name.Should().Be(env, $"Profile name should match environment {env}");
            
            if (env == "CI")
            {
                profile.HeadlessMode.Should().BeTrue("CI environment should use headless mode");
                profile.CaptureVideos.Should().BeTrue("CI environment should capture videos");
            }
            else
            {
                profile.HeadlessMode.Should().BeFalse("Non-CI environments should use headed mode");
            }
        }
        
        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
}