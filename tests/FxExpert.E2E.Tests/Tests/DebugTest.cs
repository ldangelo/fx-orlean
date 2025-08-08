using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class DebugTest : PageTest
{
    [Test]
    public async Task Debug_HomePage_ShouldShowActualContent()
    {
        // Create screenshots directory
        Directory.CreateDirectory("debug-screenshots");
        
        // Navigate to the home page
        await Page.GotoAsync("https://localhost:8501");
        
        // Take screenshot of what we actually see
        await Page.ScreenshotAsync(new()
        {
            Path = "debug-screenshots/homepage-actual.png",
            FullPage = true
        });
        
        // Get the page title and content
        var title = await Page.TitleAsync();
        Console.WriteLine($"Actual page title: '{title}'");
        
        // Get all text content
        var bodyText = await Page.Locator("body").InnerTextAsync();
        Console.WriteLine($"Body text preview: {bodyText.Substring(0, Math.Min(500, bodyText.Length))}...");
        
        // Look for any forms or textareas
        var textareas = await Page.Locator("textarea").CountAsync();
        var inputs = await Page.Locator("input").CountAsync();
        var forms = await Page.Locator("form").CountAsync();
        
        Console.WriteLine($"Found {textareas} textareas, {inputs} inputs, {forms} forms");
        
        // Get all textarea placeholders if any exist
        if (textareas > 0)
        {
            for (int i = 0; i < textareas; i++)
            {
                var placeholder = await Page.Locator("textarea").Nth(i).GetAttributeAsync("placeholder");
                Console.WriteLine($"Textarea {i} placeholder: '{placeholder}'");
            }
        }
        
        // Check if we're on the login page
        var isLoginPage = await Page.Locator("text=Sign in").CountAsync() > 0;
        Console.WriteLine($"Appears to be login page: {isLoginPage}");
    }
}