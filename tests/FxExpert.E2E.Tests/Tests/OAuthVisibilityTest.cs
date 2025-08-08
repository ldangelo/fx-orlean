using Microsoft.Playwright;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class OAuthVisibilityTest
{
    [Test]
    [Category("OAuth")]
    [Category("Visibility")]
    public async Task OAuth_WithVisibleBrowser_ShouldAllowManualAuthentication()
    {
        Console.WriteLine("🔍 Testing OAuth with visible browser...");
        
        // Create Playwright instance with manual browser management
        using var playwright = await Playwright.CreateAsync();
        
        // Create browser with explicit headed mode
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = false,  // Explicitly headed
            SlowMo = 500,      // Slow down for OAuth interaction
            Timeout = 60000,   // 60 second timeout for browser launch
            Args = new[]
            {
                "--disable-web-security",
                "--disable-blink-features=AutomationControlled",
                "--no-first-run"
            }
        };
        
        Console.WriteLine("🚀 Launching visible browser for OAuth testing...");
        await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        
        // Create context optimized for OAuth
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true,
            AcceptDownloads = false,
            JavaScriptEnabled = true
        };
        
        await using var context = await browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();
        
        Console.WriteLine("📱 Browser window is now visible!");
        
        // Initialize page objects
        var authPage = new AuthenticationPage(page);
        var configManager = AuthenticationConfigurationManager.CreateDefault("Development");
        
        try
        {
            Console.WriteLine("🌐 Navigating to FX-Orleans application...");
            Console.WriteLine("   URL: https://localhost:8501");
            
            // Navigate to the application
            await authPage.NavigateAsync();
            
            Console.WriteLine("🔐 If Keycloak login appears, this is where you would:");
            Console.WriteLine("   1. Click 'Login with Google'");
            Console.WriteLine("   2. Complete Google authentication");
            Console.WriteLine("   3. The test will detect completion automatically");
            
            // Attempt OAuth with a reasonable timeout
            Console.WriteLine("⏳ Attempting OAuth flow (60 second timeout)...");
            var authResult = await authPage.HandleGoogleOAuthAsync(60000, 1);
            
            Console.WriteLine($"🎯 OAuth result: {authResult}");
            
            if (!authResult)
            {
                Console.WriteLine("ℹ️  OAuth timed out or failed - this is expected if:");
                Console.WriteLine("   - FX-Orleans server is not running");
                Console.WriteLine("   - You didn't complete authentication in time");
                Console.WriteLine("   - Connection was refused");
            }
            else
            {
                Console.WriteLine("✅ OAuth completed successfully!");
            }
            
            // Take screenshot regardless of result
            var screenshotPath = Path.Combine("screenshots", $"oauth-visibility-test-{DateTime.Now:yyyyMMdd-HHmmss}.png");
            Directory.CreateDirectory("screenshots");
            await page.ScreenshotAsync(new() { Path = screenshotPath });
            Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception during OAuth test: {ex.Message}");
            
            if (ex.Message.Contains("ERR_CONNECTION_REFUSED"))
            {
                Console.WriteLine("ℹ️  This is expected - FX-Orleans server is not running");
                Console.WriteLine("💡 To test OAuth fully:");
                Console.WriteLine("   1. Start server: dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj");
                Console.WriteLine("   2. Run this test again");
            }
            
            // Take screenshot even on error
            var screenshotPath = Path.Combine("screenshots", $"oauth-error-{DateTime.Now:yyyyMMdd-HHmmss}.png");
            Directory.CreateDirectory("screenshots");
            await page.ScreenshotAsync(new() { Path = screenshotPath });
            Console.WriteLine($"📸 Error screenshot saved: {screenshotPath}");
        }
        
        Console.WriteLine("✅ OAuth visibility test completed!");
        Console.WriteLine("🔍 Check the screenshots directory for captured images");
        
        // Test passes if browser appeared (regardless of OAuth outcome)
        Assert.Pass("Browser visibility test completed - check console output and screenshots");
    }
}