using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class HomePage : BasePage
{
    public HomePage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    // Locators
    private ILocator ProblemDescriptionField => Page.Locator("[data-testid='problem-description']").Or(Page.Locator("textarea[placeholder*='describe']")).Or(Page.Locator("textarea").First);
    private ILocator IndustrySelect => Page.Locator("[data-testid='industry-select']").Or(Page.Locator("select")).Or(Page.GetByText("Industry").Locator("..").Locator("select"));
    private ILocator PrioritySelect => Page.Locator("[data-testid='priority-select']").Or(Page.Locator("[placeholder*='priority']")).Or(Page.GetByText("Priority").Locator("..").Locator("select"));
    private ILocator SubmitButton => Page.Locator("[data-testid='submit-problem']").Or(Page.Locator("button[type='submit']")).Or(Page.GetByRole(AriaRole.Button, new() { Name = "Find Expert" }));
    private ILocator LoadingIndicator => Page.Locator("[data-testid='loading']").Or(Page.Locator(".mud-progress-linear"));
    private ILocator PartnerResults => Page.Locator("[data-testid='partner-results']").Or(Page.Locator(".partner-card"));

    // Navigation elements
    private ILocator HomeLink => Page.Locator("a[href='/']").Or(Page.GetByText("Home"));
    private ILocator AboutLink => Page.Locator("a[href='/about']").Or(Page.GetByText("About"));
    private ILocator ExpertsLink => Page.Locator("a[href='/experts']").Or(Page.GetByText("Our Experts"));
    private ILocator ContactLink => Page.Locator("a[href='/contact']").Or(Page.GetByText("Contact"));

    // Header elements  
    private ILocator UserMenuButton => Page.Locator("[data-testid='user-menu']").Or(Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "person" }));
    private ILocator ThemeToggleButton => Page.Locator("[data-testid='theme-toggle']").Or(Page.GetByRole(AriaRole.Button).Filter(new() { Has = Page.Locator("svg") }));
    private ILocator SignInButton => Page.Locator("a[href*='login']").Or(Page.GetByText("Sign In"));

    public async Task SubmitProblemDescriptionAsync(string description, string industry = "Technology", string priority = "High")
    {
        await FillProblemDescriptionAsync(description);
        
        if (!string.IsNullOrEmpty(industry))
            await SelectIndustryAsync(industry);
            
        if (!string.IsNullOrEmpty(priority))
            await SelectPriorityAsync(priority);
            
        await ClickSubmitAsync();
    }

    public async Task FillProblemDescriptionAsync(string description)
    {
        await ProblemDescriptionField.WaitForAsync();
        await ProblemDescriptionField.FillAsync(description);
    }

    public async Task SelectIndustryAsync(string industry)
    {
        try
        {
            await IndustrySelect.WaitForAsync(new() { Timeout = 5000 });
            await IndustrySelect.SelectOptionAsync(industry);
        }
        catch (TimeoutException)
        {
            // Fallback: try to click on industry dropdown or button
            var industryDropdown = Page.GetByText("Industry").Or(Page.Locator("[placeholder*='industry']"));
            if (await industryDropdown.CountAsync() > 0)
            {
                await industryDropdown.ClickAsync();
                await Page.GetByText(industry).ClickAsync();
            }
        }
    }

    public async Task SelectPriorityAsync(string priority)
    {
        try
        {
            await PrioritySelect.WaitForAsync(new() { Timeout = 5000 });
            await PrioritySelect.SelectOptionAsync(priority);
        }
        catch (TimeoutException)
        {
            // Fallback: try to click on priority dropdown or button
            var priorityDropdown = Page.GetByText("Priority").Or(Page.Locator("[placeholder*='priority']"));
            if (await priorityDropdown.CountAsync() > 0)
            {
                await priorityDropdown.ClickAsync();
                await Page.GetByText(priority).ClickAsync();
            }
        }
    }

    public async Task ClickSubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task WaitForPartnerResultsAsync(int timeoutMs = 15000)
    {
        await LoadingIndicator.WaitForAsync(new() { Timeout = 5000 });
        await PartnerResults.WaitForAsync(new() { Timeout = timeoutMs });
    }

    public async Task<int> GetPartnerResultsCountAsync()
    {
        return await PartnerResults.CountAsync();
    }

    public async Task<string[]> GetPartnerNamesAsync()
    {
        var partners = await PartnerResults.AllAsync();
        var names = new List<string>();
        
        foreach (var partner in partners)
        {
            var nameElement = partner.Locator("h3, h4, h5, h6, .partner-name").First;
            if (await nameElement.CountAsync() > 0)
            {
                names.Add(await nameElement.TextContentAsync() ?? "");
            }
        }
        
        return names.ToArray();
    }

    public async Task ClickPartnerAsync(int index = 0)
    {
        var partners = await PartnerResults.AllAsync();
        if (index < partners.Count)
        {
            await partners[index].ClickAsync();
        }
    }

    // Navigation methods
    public async Task ClickSignInAsync()
    {
        await SignInButton.ClickAsync();
    }

    public async Task ToggleThemeAsync()
    {
        await ThemeToggleButton.ClickAsync();
    }

    public async Task OpenUserMenuAsync()
    {
        await UserMenuButton.ClickAsync();
    }

    // Validation methods
    public async Task AssertPartnerResultsVisibleAsync()
    {
        await PartnerResults.First.WaitForAsync();
        (await GetPartnerResultsCountAsync()).Should().BeGreaterThan(0);
    }

    public async Task AssertLoadingIndicatorVisibleAsync()
    {
        await LoadingIndicator.WaitForAsync();
    }

    public async Task AssertHomePageLoadedAsync()
    {
        await AssertPageTitleContainsAsync("Fortium");
        await ProblemDescriptionField.WaitForAsync();
    }
}