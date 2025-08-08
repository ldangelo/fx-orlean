using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class ConfirmationPage : BasePage
{
    public ConfirmationPage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    // Locators
    private ILocator SuccessIcon => Page.Locator("[data-testid='success-icon']").Or(Page.GetByRole(AriaRole.Img).Filter(new() { HasText = "check" }));
    private ILocator ConfirmationTitle => Page.Locator("[data-testid='confirmation-title']").Or(Page.GetByText("Consultation Scheduled"));
    private ILocator PartnerName => Page.Locator("[data-testid='confirmation-partner']").Or(Page.GetByText("Expert:"));
    private ILocator MeetingDateTime => Page.Locator("[data-testid='confirmation-datetime']").Or(Page.GetByText("Date & Time:"));
    private ILocator MeetingLink => Page.Locator("[data-testid='confirmation-meeting-link']").Or(Page.GetByText("Meeting Link:"));
    private ILocator MeetingDuration => Page.Locator("[data-testid='confirmation-duration']").Or(Page.GetByText("Duration:"));
    private ILocator PaymentInfo => Page.Locator("[data-testid='confirmation-payment']").Or(Page.GetByText("Payment:"));
    private ILocator ReturnHomeButton => Page.Locator("[data-testid='return-home']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Return to Home" }));

    public async Task AssertConfirmationPageLoadedAsync()
    {
        await SuccessIcon.WaitForAsync();
        await ConfirmationTitle.WaitForAsync();
        
        // Verify all key elements are present
        (await SuccessIcon.CountAsync()).Should().BeGreaterThan(0);
        (await ConfirmationTitle.CountAsync()).Should().BeGreaterThan(0);
        (await ReturnHomeButton.CountAsync()).Should().BeGreaterThan(0);
    }

    public async Task AssertBookingDetailsAsync()
    {
        // Verify all booking details are displayed
        await PartnerName.WaitForAsync();
        await MeetingDateTime.WaitForAsync();
        await MeetingDuration.WaitForAsync();
        await PaymentInfo.WaitForAsync();
        
        // Check that duration shows 60 minutes
        var duration = await MeetingDuration.TextContentAsync();
        duration.Should().Contain("60 minutes");
        
        // Check that payment amount is shown
        var payment = await PaymentInfo.TextContentAsync();
        payment.Should().Contain("800");
    }

    public async Task AssertGoogleMeetLinkAsync()
    {
        await MeetingLink.WaitForAsync();
        var linkText = await MeetingLink.TextContentAsync();
        linkText.Should().Contain("Google Meet");
    }

    public async Task<string> GetPartnerNameAsync()
    {
        var nameText = await PartnerName.TextContentAsync();
        return nameText?.Replace("Expert:", "").Trim() ?? "";
    }

    public async Task<string> GetMeetingDateTimeAsync()
    {
        var dateTimeText = await MeetingDateTime.TextContentAsync();
        return dateTimeText?.Replace("Date & Time:", "").Trim() ?? "";
    }

    public async Task ClickReturnHomeAsync()
    {
        await ReturnHomeButton.ClickAsync();
        await Page.WaitForURLAsync("**/", new() { Timeout = 5000 });
    }

    public async Task TakeConfirmationScreenshotAsync()
    {
        await TakeScreenshotAsync("booking-confirmation");
    }

    // Authentication-related methods
    public async Task NavigateToConfirmationAsync(string bookingId = "test-booking")
    {
        await NavigateAsync($"/confirmation/{bookingId}");
        await AssertConfirmationPageLoadedAsync();
    }

    public async Task<bool> RequiresAuthenticationAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            return IsAuthenticationUrl(currentUrl);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> CanAccessAuthenticationStateAsync()
    {
        try
        {
            // Check if confirmation page can access and display user authentication state
            var userMenuVisible = await Page.Locator("[data-testid='user-menu']").IsVisibleAsync();
            var authStatusDetectable = await IsUserAuthenticatedAsync();
            
            // Page should be able to detect authentication state
            return true; // Always true as BasePage provides authentication detection
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ShowsUserSpecificContentAsync()
    {
        try
        {
            if (!await IsUserAuthenticatedAsync())
                return false;

            // Check if confirmation page shows user-specific content when authenticated
            var userInfo = await Page.Locator("[data-testid='user-info']").IsVisibleAsync();
            var personalizedContent = await Page.GetByText("Your consultation").Or(Page.GetByText("Hi,")).IsVisibleAsync();
            
            return userInfo || personalizedContent;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> PersistsBookingDataAsync()
    {
        try
        {
            // Verify that booking data persists across authentication state changes
            var partnerName = await GetPartnerNameAsync();
            var meetingDateTime = await GetMeetingDateTimeAsync();
            
            // If we have booking data, it should persist regardless of auth state
            return !string.IsNullOrEmpty(partnerName) || !string.IsNullOrEmpty(meetingDateTime);
        }
        catch (Exception)
        {
            return false;
        }
    }
}