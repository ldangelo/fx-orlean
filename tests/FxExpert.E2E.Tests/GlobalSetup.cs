using Microsoft.Playwright;
using NUnit.Framework;

namespace FxExpert.E2E.Tests;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public async Task Setup()
    {
        // Install Playwright browsers if not already installed
        Microsoft.Playwright.Program.Main(new[] { "install" });
        
        // Wait a bit for installation to complete
        await Task.Delay(1000);
    }
}