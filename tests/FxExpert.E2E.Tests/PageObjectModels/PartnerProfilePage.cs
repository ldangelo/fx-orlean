using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class PartnerProfilePage : BasePage
{
    public PartnerProfilePage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    // Locators
    private ILocator PartnerName => Page.Locator("[data-testid='partner-name']").Or(Page.Locator("h1, h2, h3, h4").First);
    private ILocator PartnerTitle => Page.Locator("[data-testid='partner-title']").Or(Page.GetByText("Chief").Or(Page.GetByText("CTO")).Or(Page.GetByText("CIO")));
    private ILocator PartnerBio => Page.Locator("[data-testid='partner-bio']").Or(Page.Locator("p").Filter(new() { HasText = "experience" }));
    private ILocator SkillsChips => Page.Locator("[data-testid='partner-skills']").Or(Page.Locator(".mud-chip"));
    private ILocator ScheduleButton => Page.Locator("[data-testid='schedule-consultation']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Schedule a Consultation" }));
    
    // Scheduling panel
    private ILocator DatePicker => Page.Locator("[data-testid='date-picker']").Or(Page.Locator("input[placeholder*='date']"));
    private ILocator TimeSelect => Page.Locator("[data-testid='time-select']").Or(Page.Locator("select").Filter(new() { HasText = "AM" }));
    private ILocator MeetingTopicField => Page.Locator("[data-testid='meeting-topic']").Or(Page.Locator("textarea[placeholder*='topic']"));
    private ILocator ProceedToPaymentButton => Page.Locator("[data-testid='proceed-payment']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Proceed to Payment" }));
    
    // Payment section
    private ILocator PaymentForm => Page.Locator("[data-testid='payment-form']").Or(Page.Locator("#payment-element"));
    private ILocator AuthorizePaymentButton => Page.Locator("[data-testid='authorize-payment']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Authorize Payment" }));
    private ILocator BackToScheduleButton => Page.Locator("[data-testid='back-to-schedule']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Back to Schedule" }));

    public async Task AssertPartnerProfileLoadedAsync()
    {
        await PartnerName.WaitForAsync();
        await PartnerTitle.WaitForAsync();
        await ScheduleButton.WaitForAsync();
    }

    public async Task<string> GetPartnerNameAsync()
    {
        return await PartnerName.TextContentAsync() ?? "";
    }

    public async Task<string> GetPartnerTitleAsync()
    {
        return await PartnerTitle.TextContentAsync() ?? "";
    }

    public async Task<string[]> GetPartnerSkillsAsync()
    {
        var skills = await SkillsChips.AllAsync();
        var skillTexts = new List<string>();
        
        foreach (var skill in skills)
        {
            var text = await skill.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
                skillTexts.Add(text);
        }
        
        return skillTexts.ToArray();
    }

    public async Task ClickScheduleConsultationAsync()
    {
        await ScheduleButton.ClickAsync();
        await WaitForSchedulingPanelAsync();
    }

    public async Task WaitForSchedulingPanelAsync()
    {
        await DatePicker.WaitForAsync();
        await TimeSelect.WaitForAsync();
        await MeetingTopicField.WaitForAsync();
    }

    public async Task FillSchedulingDetailsAsync(string date, string time, string topic)
    {
        // Select date
        await DatePicker.ClickAsync();
        // For simplicity, select the first available date (in a real test, you'd parse the date)
        await Page.GetByText("15").Or(Page.GetByText("16")).Or(Page.GetByText("17")).First.ClickAsync();
        
        // Select time
        await TimeSelect.ClickAsync();
        await Page.GetByText(time).ClickAsync();
        
        // Fill meeting topic
        await MeetingTopicField.FillAsync(topic);
    }

    public async Task ClickProceedToPaymentAsync()
    {
        await ProceedToPaymentButton.ClickAsync();
        await WaitForPaymentFormAsync();
    }

    public async Task WaitForPaymentFormAsync()
    {
        await PaymentForm.WaitForAsync(new() { Timeout = 10000 });
        await AuthorizePaymentButton.WaitForAsync();
    }

    public async Task FillPaymentDetailsAsync(string cardNumber = "4242424242424242", string expiry = "12/34", string cvc = "123", string zip = "12345")
    {
        // Wait for Stripe Elements to load
        await Task.Delay(2000);
        
        // Switch to Stripe iframe for card number
        var cardFrame = Page.FrameLocator("iframe[name*='__privateStripeFrame']");
        await cardFrame.Locator("input[placeholder*='1234']").FillAsync(cardNumber);
        
        // Fill expiry date
        await cardFrame.Locator("input[placeholder*='MM']").FillAsync(expiry);
        
        // Fill CVC
        await cardFrame.Locator("input[placeholder*='CVC']").FillAsync(cvc);
        
        // Fill ZIP code
        await cardFrame.Locator("input[placeholder*='ZIP']").FillAsync(zip);
    }

    public async Task ClickAuthorizePaymentAsync()
    {
        await AuthorizePaymentButton.ClickAsync();
    }

    public async Task WaitForPaymentProcessingAsync()
    {
        // Wait for payment processing (loading state)
        await Page.GetByText("Processing Payment").WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task AssertPaymentSuccessAsync()
    {
        // Check if we're redirected to confirmation page or success state
        await Page.WaitForURLAsync("**/confirmation/**", new() { Timeout = 15000 });
    }

    public async Task CompleteBookingWorkflowAsync(string topic = "Technology strategy consultation")
    {
        await ClickScheduleConsultationAsync();
        await FillSchedulingDetailsAsync("", "10:00 AM", topic);
        await ClickProceedToPaymentAsync();
        await FillPaymentDetailsAsync();
        await ClickAuthorizePaymentAsync();
        await WaitForPaymentProcessingAsync();
        await AssertPaymentSuccessAsync();
    }

    // Authentication-related methods
    public async Task NavigateToPartnerProfileAsync(int partnerId = 1)
    {
        await NavigateAsync($"/partners/{partnerId}");
        await AssertPartnerProfileLoadedAsync();
    }

    public async Task<bool> RequiresAuthenticationAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            if (IsAuthenticationUrl(currentUrl))
                return true;

            // Check if scheduling requires authentication
            await ScheduleButton.ClickAsync(new() { Timeout = 5000 });
            
            // If redirected to auth page after clicking schedule
            await Task.Delay(1000);
            var newUrl = Page.Url;
            return IsAuthenticationUrl(newUrl);
        }
        catch (Exception)
        {
            // If we can't click schedule button, might need auth
            return true;
        }
    }

    public async Task<bool> CanAccessWithoutAuthenticationAsync()
    {
        try
        {
            // Check if we can view partner profile without authentication
            var nameVisible = await PartnerName.IsVisibleAsync();
            var titleVisible = await PartnerTitle.IsVisibleAsync();
            var scheduleVisible = await ScheduleButton.IsVisibleAsync();
            
            return nameVisible && titleVisible && scheduleVisible;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> HandlesAuthenticationStateAsync()
    {
        try
        {
            // Check if page handles authentication state gracefully
            var isAuthenticated = await IsUserAuthenticatedAsync();
            var pageLoaded = await IsPageLoadedAsync();
            
            // Page should load regardless of auth state
            return pageLoaded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ShowsAuthenticatedFeaturesAsync()
    {
        try
        {
            if (!await IsUserAuthenticatedAsync())
                return false;

            // Check for authenticated-only features (like user preferences, history, etc.)
            var userMenuVisible = await Page.Locator("[data-testid='user-menu']").IsVisibleAsync();
            var dashboardLinkVisible = await Page.GetByText("Dashboard").IsVisibleAsync();
            
            return userMenuVisible || dashboardLinkVisible;
        }
        catch (Exception)
        {
            return false;
        }
    }
}