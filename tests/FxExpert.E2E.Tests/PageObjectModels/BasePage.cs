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
}