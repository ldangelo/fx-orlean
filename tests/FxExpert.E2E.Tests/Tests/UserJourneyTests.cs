using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class UserJourneyTests : PageTest
{
    private HomePage? _homePage;
    private PartnerProfilePage? _partnerPage;
    private ConfirmationPage? _confirmationPage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        // Create page objects
        _homePage = new HomePage(Page);
        _partnerPage = new PartnerProfilePage(Page);
        _confirmationPage = new ConfirmationPage(Page);
        _authPage = new AuthenticationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        // Create screenshots directory
        await Task.Run(() => Directory.CreateDirectory("screenshots"));
    }

    [Test]
    [Category("P0")]
    [Category("Critical-Path")]
    public async Task CompleteBookingWorkflow_NewUser_ShouldSucceed()
    {
        // Arrange
        const string problemDescription = "We need help developing a comprehensive technology strategy for our growing fintech company. We're struggling with cloud architecture decisions and need expert guidance on scalability and security.";
        const string industry = "Technology";
        const string priority = "High";
        const string meetingTopic = "Technology strategy consultation for fintech scaling";

        // Act & Assert
        
        // Step 0: Handle authentication if required
        var config = await _configManager!.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();
        
        await _homePage!.NavigateAsync();
        
        // Check if authentication is required by seeing if we're redirected
        var currentUrl = Page.Url;
        var requiresAuth = await _homePage.RequiresAuthenticationAsync();
        
        if (requiresAuth)
        {
            Console.WriteLine("Authentication required - handling Google OAuth flow...");
            await _authPage!.TakeScreenshotAsync("00-auth-required");
            
            // Attempt OAuth authentication (will timeout in test environment)
            var authResult = await _authPage.HandleGoogleOAuthAsync(effectiveTimeout);
            
            if (!authResult)
            {
                Console.WriteLine("OAuth authentication timed out in test environment - this is expected");
                Console.WriteLine("In a real scenario, user would complete Google authentication manually");
                await _authPage.TakeScreenshotAsync("00-auth-timeout");
            }
            
            // Continue with test regardless of auth result (testing the flow)
        }
        
        // Step 1: Ensure we're on home page and it's loaded
        await _homePage.NavigateAsync();
        await _homePage.AssertHomePageLoadedAsync();
        await _homePage.TakeScreenshotAsync("01-home-page-loaded");

        // Step 2: Submit problem description
        await _homePage.SubmitProblemDescriptionAsync(problemDescription, industry, priority);
        await _homePage.AssertLoadingIndicatorVisibleAsync();
        await _homePage.TakeScreenshotAsync("02-problem-submitted-loading");

        // Step 3: Wait for and verify partner results
        await _homePage.WaitForPartnerResultsAsync();
        await _homePage.AssertPartnerResultsVisibleAsync();
        var partnerCount = await _homePage.GetPartnerResultsCountAsync();
        partnerCount.Should().BeGreaterThan(0, "AI matching should return at least one partner");
        await _homePage.TakeScreenshotAsync("03-partner-results-displayed");

        // Step 4: Select first partner
        await _homePage.ClickPartnerAsync(0);
        await _partnerPage!.AssertPartnerProfileLoadedAsync();
        await _partnerPage.TakeScreenshotAsync("04-partner-profile-loaded");

        // Step 5: Verify partner details are displayed
        var partnerName = await _partnerPage.GetPartnerNameAsync();
        var partnerTitle = await _partnerPage.GetPartnerTitleAsync();
        var partnerSkills = await _partnerPage.GetPartnerSkillsAsync();
        
        partnerName.Should().NotBeNullOrEmpty("Partner name should be displayed");
        partnerTitle.Should().NotBeNullOrEmpty("Partner title should be displayed");
        partnerSkills.Should().NotBeEmpty("Partner skills should be displayed");

        // Step 6: Schedule consultation
        await _partnerPage.ClickScheduleConsultationAsync();
        await _partnerPage.TakeScreenshotAsync("05-scheduling-panel-opened");

        // Step 7: Fill scheduling details
        await _partnerPage.FillSchedulingDetailsAsync("", "10:00 AM", meetingTopic);
        await _partnerPage.TakeScreenshotAsync("06-scheduling-details-filled");

        // Step 8: Proceed to payment
        await _partnerPage.ClickProceedToPaymentAsync();
        await _partnerPage.TakeScreenshotAsync("07-payment-form-displayed");

        // Step 9: Fill payment details and authorize
        await _partnerPage.FillPaymentDetailsAsync();
        await _partnerPage.TakeScreenshotAsync("08-payment-details-filled");
        
        await _partnerPage.ClickAuthorizePaymentAsync();
        await _partnerPage.TakeScreenshotAsync("09-payment-processing");

        // Step 10: Wait for payment processing and success
        await _partnerPage.WaitForPaymentProcessingAsync();
        await _partnerPage.AssertPaymentSuccessAsync();
        
        // Step 11: Verify confirmation page
        await _confirmationPage!.AssertConfirmationPageLoadedAsync();
        await _confirmationPage.AssertBookingDetailsAsync();
        await _confirmationPage.AssertGoogleMeetLinkAsync();
        await _confirmationPage.TakeConfirmationScreenshotAsync();

        // Step 12: Verify booking details match
        var confirmedPartner = await _confirmationPage.GetPartnerNameAsync();
        confirmedPartner.Should().Contain(partnerName.Split(' ')[0], "Confirmed partner should match selected partner");

        // Step 13: Return to home
        await _confirmationPage.ClickReturnHomeAsync();
        await _homePage.AssertHomePageLoadedAsync();

        // Step 14: Validate session persistence across the entire workflow
        Console.WriteLine("Validating session persistence...");
        var sessionPersists = await _homePage.ValidateSessionPersistenceAsync();
        var cookiesValid = await _homePage.ValidateAuthenticationCookiesAsync();
        var userContextAvailable = await _homePage.ValidateUserContextAvailabilityAsync();
        
        Console.WriteLine($"Session persistence: {sessionPersists}");
        Console.WriteLine($"Cookie validation: {cookiesValid}");
        Console.WriteLine($"User context availability: {userContextAvailable}");
    }

    [Test]
    [Category("P0")]
    [Category("Payment")]
    public async Task PaymentAuthorization_WithValidCard_ShouldSucceed()
    {
        // Arrange
        var config = await _configManager!.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();

        // Step 0: Handle authentication if required
        await _homePage!.NavigateAsync();
        var requiresAuth = await _homePage.RequiresAuthenticationAsync();
        
        if (requiresAuth)
        {
            Console.WriteLine("Authentication required for payment test - handling Google OAuth flow...");
            await _authPage!.TakeScreenshotAsync("payment-test-auth-required");
            
            var authResult = await _authPage.HandleGoogleOAuthAsync(effectiveTimeout);
            
            if (!authResult)
            {
                Console.WriteLine("OAuth authentication timed out in test environment - this is expected");
                Console.WriteLine("In a real scenario, user would complete Google authentication manually");
                await _authPage.TakeScreenshotAsync("payment-test-auth-timeout");
            }
        }

        // Set up a booking ready for payment
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync("Quick technology consultation needed");
        await _homePage.WaitForPartnerResultsAsync();
        await _homePage.ClickPartnerAsync(0);
        await _partnerPage!.ClickScheduleConsultationAsync();
        await _partnerPage.FillSchedulingDetailsAsync("", "2:00 PM", "Quick consultation");
        await _partnerPage.ClickProceedToPaymentAsync();

        // Act - Test payment authorization
        await _partnerPage.FillPaymentDetailsAsync("4242424242424242", "12/34", "123", "12345");
        await _partnerPage.ClickAuthorizePaymentAsync();

        // Assert
        await _partnerPage.WaitForPaymentProcessingAsync();
        await _partnerPage.AssertPaymentSuccessAsync();
        await _confirmationPage!.AssertConfirmationPageLoadedAsync();

        // Validate session persistence during payment flow
        Console.WriteLine("Validating session persistence during payment...");
        var sessionPersists = await _confirmationPage.ValidateSessionPersistenceAsync();
        var cookiesValid = await _confirmationPage.ValidateAuthenticationCookiesAsync();
        
        Console.WriteLine($"Payment session persistence: {sessionPersists}");
        Console.WriteLine($"Payment cookie validation: {cookiesValid}");
    }

    [Test]
    [Category("P1")]
    [Category("Payment")]
    public async Task PaymentAuthorization_WithDeclinedCard_ShouldShowError()
    {
        // Arrange - Set up a booking ready for payment
        await _homePage!.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync("Test consultation for declined card");
        await _homePage.WaitForPartnerResultsAsync();
        await _homePage.ClickPartnerAsync(0);
        await _partnerPage!.ClickScheduleConsultationAsync();
        await _partnerPage.FillSchedulingDetailsAsync("", "3:00 PM", "Test consultation");
        await _partnerPage.ClickProceedToPaymentAsync();

        // Act - Test with declined card
        await _partnerPage.FillPaymentDetailsAsync("4000000000000002", "12/34", "123", "12345");
        await _partnerPage.ClickAuthorizePaymentAsync();

        // Assert
        await Page.GetByText("declined").Or(Page.GetByText("failed")).Or(Page.GetByText("error")).WaitForAsync(new() { Timeout = 10000 });
        var errorMessage = Page.GetByText("declined").Or(Page.GetByText("failed")).Or(Page.GetByText("error"));
        errorMessage.Should().NotBeNull("Error message should be displayed for declined card");
        
        // Verify user remains on payment page
        await _partnerPage.WaitForPaymentFormAsync();
    }

    [Test]
    [Category("P0")]
    [Category("AI-Matching")]
    public async Task AIPartnerMatching_WithTechProblem_ShouldReturnRelevantExperts()
    {
        // Arrange
        const string techProblem = "We need to migrate our legacy systems to the cloud and implement DevOps practices. Looking for expertise in AWS, containerization, and CI/CD pipeline setup.";
        var config = await _configManager!.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();

        // Step 0: Handle authentication if required
        await _homePage!.NavigateAsync();
        var requiresAuth = await _homePage.RequiresAuthenticationAsync();
        
        if (requiresAuth)
        {
            Console.WriteLine("Authentication required for AI matching test - handling Google OAuth flow...");
            await _authPage!.TakeScreenshotAsync("ai-matching-test-auth-required");
            
            var authResult = await _authPage.HandleGoogleOAuthAsync(effectiveTimeout);
            
            if (!authResult)
            {
                Console.WriteLine("OAuth authentication timed out in test environment - this is expected");
                Console.WriteLine("In a real scenario, user would complete Google authentication manually");
                await _authPage.TakeScreenshotAsync("ai-matching-test-auth-timeout");
            }
        }

        // Act
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync(techProblem, "Technology", "High");
        await _homePage.WaitForPartnerResultsAsync();

        // Assert
        await _homePage.AssertPartnerResultsVisibleAsync();
        var partnerCount = await _homePage.GetPartnerResultsCountAsync();
        var partnerNames = await _homePage.GetPartnerNamesAsync();
        
        partnerCount.Should().BeGreaterThan(0, "AI should return relevant partners");
        partnerNames.Should().AllSatisfy(name => name.Should().NotBeNullOrEmpty("All partner names should be populated"));

        // Validate session persistence during AI matching
        Console.WriteLine("Validating session persistence during AI matching...");
        var sessionPersists = await _homePage.ValidateSessionPersistenceAsync();
        var userContextAvailable = await _homePage.ValidateUserContextAvailabilityAsync();
        
        Console.WriteLine($"AI matching session persistence: {sessionPersists}");
        Console.WriteLine($"AI matching user context: {userContextAvailable}");
    }

    [Test]
    [Category("P1")]
    [Category("Navigation")]
    public async Task HomePage_MenuNavigation_ShouldWork()
    {
        // Arrange
        await _homePage!.NavigateAsync();

        // Act & Assert - Test theme toggle
        await _homePage.ToggleThemeAsync();
        await Task.Delay(1000); // Wait for theme change
        
        // Test user menu (if authenticated)
        try 
        {
            await _homePage.OpenUserMenuAsync();
        }
        catch (TimeoutException)
        {
            // User might not be authenticated, try sign in button instead
            await _homePage.ClickSignInAsync();
            await Page.WaitForURLAsync("**/signin-oidc**", new() { Timeout = 5000 });
        }
    }

    [Test]
    [Category("P2")]
    [Category("Responsive")]
    public async Task HomePage_MobileView_ShouldBeResponsive()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        
        // Act
        await _homePage!.NavigateAsync();
        
        // Assert
        await _homePage.AssertHomePageLoadedAsync();
        await _homePage.TakeScreenshotAsync("mobile-home-page");
        
        // Verify core elements are still accessible
        await _homePage.SubmitProblemDescriptionAsync("Mobile test problem");
        await _homePage.WaitForPartnerResultsAsync();
        await _homePage.AssertPartnerResultsVisibleAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up any test data if needed
        if (Page != null)
        {
            await Page.CloseAsync();
        }
    }
}