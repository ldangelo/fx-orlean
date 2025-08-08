using Microsoft.Playwright;
using NUnit.Framework;
using FluentAssertions;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class BrowserVisibilityTest
{
    [Test]
    [Category("BrowserTest")]
    public async Task Browser_ShouldAppearInHeadedMode()
    {
        Console.WriteLine("🔍 Testing browser visibility...");
        
        // Create Playwright instance
        using var playwright = await Playwright.CreateAsync();
        
        // Create browser with explicit headed mode
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = false, // Explicitly set to false
            SlowMo = 1000,    // Slow down for visibility
            Timeout = 30000   // 30 second timeout
        };
        
        Console.WriteLine("🚀 Launching browser in headed mode...");
        await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        
        // Create context
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true
        };
        
        await using var context = await browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();
        
        Console.WriteLine("📱 Browser window should be visible now!");
        Console.WriteLine("🌐 Navigating to data URL (no internet required)...");
        
        // Navigate to a simple data URL that doesn't require internet
        await page.GotoAsync("data:text/html,<html><head><title>Browser Test</title></head><body><h1>✅ Browser is Visible!</h1><p>This browser window should be visible to you.</p></body></html>");
        
        // Wait a bit so you can see the browser
        Console.WriteLine("⏳ Waiting 8 seconds for you to see the browser window...");
        Console.WriteLine("🔍 Look for a browser window with the title 'Browser Test'");
        await Task.Delay(8000);
        
        // Take a screenshot to verify it worked
        var screenshotPath = Path.Combine("screenshots", $"browser-visibility-test-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        Directory.CreateDirectory("screenshots");
        await page.ScreenshotAsync(new() { Path = screenshotPath });
        
        Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");
        Console.WriteLine("✅ Browser visibility test completed!");
        
        // Verify the page loaded
        var title = await page.TitleAsync();
        title.Should().Be("Browser Test", "Should have loaded our test page");
        
        // Close explicitly
        await page.CloseAsync();
    }
}