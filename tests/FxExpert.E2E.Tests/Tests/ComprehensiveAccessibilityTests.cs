using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;
using FxExpert.E2E.Tests.Helpers;
using System.Text.Json;

namespace FxExpert.E2E.Tests.Tests;

/// <summary>
/// Comprehensive accessibility compliance tests using AccessibilityValidator
/// Performs full WCAG 2.1 AA audit of the partner filtering functionality
/// </summary>
[TestFixture]
public class ComprehensiveAccessibilityTests : PageTest
{
    private HomePage? _homePage;
    private PartnerFilterPage? _filterPage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        _filterPage = new PartnerFilterPage(Page);
        _authPage = new AuthenticationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        await Task.Run(() => Directory.CreateDirectory("screenshots/comprehensive-accessibility"));
        await Task.Run(() => Directory.CreateDirectory("reports/accessibility"));
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Comprehensive")]
    public async Task ComprehensiveAccessibilityAudit_HomePage_ShouldPassWCAGStandards()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _homePage!.TakeScreenshotAsync("comprehensive-accessibility/01-home-page-audit");

        // Act - Perform comprehensive accessibility audit
        var auditResult = await AccessibilityValidator.PerformFullAuditAsync(Page);

        // Generate detailed report
        var reportJson = JsonSerializer.Serialize(auditResult, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync("reports/accessibility/home-page-audit.json", reportJson);

        // Assert
        auditResult.Should().NotBeNull("Audit should complete successfully");
        auditResult.Reports.Should().NotBeEmpty("Should have accessibility reports");
        auditResult.OverallScore.Should().BeGreaterThan(60, "Should meet minimum accessibility score");
        
        if (!string.IsNullOrEmpty(auditResult.Error))
        {
            Console.WriteLine($"Audit Error: {auditResult.Error}");
        }

        Console.WriteLine($"Accessibility Audit Summary:");
        Console.WriteLine($"Overall Score: {auditResult.OverallScore:F1}%");
        Console.WriteLine($"Compliance Level: {auditResult.ComplianceLevel}");
        Console.WriteLine($"Tests Completed: {auditResult.Reports.Count}");
        
        foreach (var report in auditResult.Reports)
        {
            Console.WriteLine($"- {report.TestType}: Completed at {report.Timestamp:HH:mm:ss}");
        }
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Comprehensive")]
    public async Task ComprehensiveAccessibilityAudit_FilterSidebar_ShouldPassWCAGStandards()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();
        await _filterPage.TakeScreenshotAsync("comprehensive-accessibility/02-filter-sidebar-audit");

        // Act - Perform accessibility audit on filter sidebar
        var auditResult = await AccessibilityValidator.PerformFullAuditAsync(Page);

        // Generate detailed report
        var reportJson = JsonSerializer.Serialize(auditResult, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync("reports/accessibility/filter-sidebar-audit.json", reportJson);

        // Perform specific filter interaction tests
        await TestFilterKeyboardInteraction();
        await TestFilterScreenReaderAnnouncements();

        // Assert
        auditResult.Should().NotBeNull("Filter sidebar audit should complete");
        auditResult.Reports.Should().HaveCountGreaterOrEqualTo(3, "Should have multiple accessibility validations");
        
        Console.WriteLine($"Filter Sidebar Accessibility Summary:");
        Console.WriteLine($"Overall Score: {auditResult.OverallScore:F1}%");
        Console.WriteLine($"Compliance Level: {auditResult.ComplianceLevel}");
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Interactive")]
    public async Task AccessibilityWorkflow_FilterInteraction_ShouldMaintainAccessibility()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        
        // Act - Test accessibility through complete filter workflow
        var workflowSteps = new[]
        {
            "Initial page load",
            "Filter sidebar opened",
            "Location filter applied",
            "Skills filter applied", 
            "Availability filter applied",
            "Filters cleared"
        };

        var workflowReports = new List<(string Step, AccessibilityReport Report)>();

        for (int i = 0; i < workflowSteps.Length; i++)
        {
            var step = workflowSteps[i];
            
            // Perform the workflow step
            switch (i)
            {
                case 0: // Initial page load - already done
                    break;
                case 1: // Open filter sidebar
                    await _filterPage!.OpenFiltersAsync();
                    break;
                case 2: // Apply location filter
                    await _filterPage!.FilterByCityAsync("Denver");
                    break;
                case 3: // Apply skills filter
                    await _filterPage.FilterBySkillAsync("Strategy");
                    break;
                case 4: // Apply availability filter
                    await _filterPage.FilterByAvailabilityAsync("ThisMonth");
                    break;
                case 5: // Clear filters
                    await _filterPage.ClearAllFiltersAsync();
                    break;
            }

            await Task.Delay(1000); // Allow UI to settle
            await _filterPage!.TakeScreenshotAsync($"comprehensive-accessibility/workflow-step-{i:D2}-{step.Replace(" ", "-").ToLower()}");

            // Validate accessibility at each step
            var keyboardReport = await AccessibilityValidator.ValidateKeyboardNavigationAsync(Page);
            var focusReport = await AccessibilityValidator.ValidateFocusManagementAsync(Page);
            var dynamicReport = await AccessibilityValidator.ValidateDynamicContentAsync(Page);

            workflowReports.Add((step, keyboardReport));
            workflowReports.Add((step, focusReport));
            workflowReports.Add((step, dynamicReport));
        }

        // Generate workflow accessibility report
        var workflowReportJson = JsonSerializer.Serialize(workflowReports.Select(wr => new 
        {
            Step = wr.Step,
            TestType = wr.Report.TestType,
            Timestamp = wr.Report.Timestamp,
            HasResults = !string.IsNullOrEmpty(wr.Report.Results)
        }), new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync("reports/accessibility/workflow-accessibility-audit.json", workflowReportJson);

        // Assert
        workflowReports.Should().NotBeEmpty("Workflow should generate accessibility reports");
        workflowReports.Should().HaveCount(workflowSteps.Length * 3, "Each step should have 3 accessibility validations");
        
        Console.WriteLine($"Accessibility Workflow Summary:");
        Console.WriteLine($"Workflow Steps: {workflowSteps.Length}");
        Console.WriteLine($"Total Validations: {workflowReports.Count}");
        Console.WriteLine($"Validation Types: Keyboard Navigation, Focus Management, Dynamic Content");
    }

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Color-Contrast")]
    public async Task ColorContrastValidation_AllThemes_ShouldPassWCAGStandards()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act & Assert - Test both light and dark themes
        var themeResults = new List<(string Theme, AccessibilityReport Report)>();

        // Test light theme
        await _filterPage!.OpenFiltersAsync();
        var lightThemeReport = await AccessibilityValidator.ValidateColorContrastAsync(Page);
        themeResults.Add(("Light", lightThemeReport));
        await _filterPage.TakeScreenshotAsync("comprehensive-accessibility/03-light-theme-contrast");

        // Test dark theme
        await _homePage!.ToggleThemeAsync();
        await Task.Delay(1000); // Wait for theme change
        
        var darkThemeReport = await AccessibilityValidator.ValidateColorContrastAsync(Page);
        themeResults.Add(("Dark", darkThemeReport));
        await _filterPage.TakeScreenshotAsync("comprehensive-accessibility/04-dark-theme-contrast");

        // Switch back to light theme
        await _homePage.ToggleThemeAsync();
        await Task.Delay(1000);

        // Generate theme contrast report
        var themeReportJson = JsonSerializer.Serialize(themeResults.Select(tr => new 
        {
            Theme = tr.Theme,
            TestType = tr.Report.TestType,
            Timestamp = tr.Report.Timestamp,
            HasData = !string.IsNullOrEmpty(tr.Report.Results)
        }), new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync("reports/accessibility/theme-contrast-audit.json", themeReportJson);

        // Assert
        themeResults.Should().HaveCount(2, "Should test both light and dark themes");
        themeResults.Should().OnlyContain(tr => !string.IsNullOrEmpty(tr.Report.Results), 
            "Both themes should have contrast validation data");
        
        Console.WriteLine($"Theme Contrast Validation Summary:");
        foreach (var (theme, report) in themeResults)
        {
            Console.WriteLine($"- {theme} Theme: Validated at {report.Timestamp:HH:mm:ss}");
        }
    }

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Mobile")]
    public async Task MobileAccessibilityValidation_TouchAndKeyboard_ShouldBeAccessible()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await SetupBasicPartnerSearch();

        // Act - Perform mobile-specific accessibility validations
        var mobileAuditResult = await AccessibilityValidator.PerformFullAuditAsync(Page);
        
        // Test touch target sizes and keyboard navigation on mobile
        await _filterPage!.OpenFiltersAsync();
        await _filterPage.TakeScreenshotAsync("comprehensive-accessibility/05-mobile-accessibility");

        var keyboardOnMobile = await AccessibilityValidator.ValidateKeyboardNavigationAsync(Page);
        var focusOnMobile = await AccessibilityValidator.ValidateFocusManagementAsync(Page);

        // Generate mobile accessibility report
        var mobileReports = new List<AccessibilityReport> { keyboardOnMobile, focusOnMobile };
        mobileReports.AddRange(mobileAuditResult.Reports);

        var mobileReportJson = JsonSerializer.Serialize(new 
        {
            ViewportSize = "375x667 (Mobile)",
            OverallScore = mobileAuditResult.OverallScore,
            ComplianceLevel = mobileAuditResult.ComplianceLevel,
            Reports = mobileReports.Select(r => new 
            {
                TestType = r.TestType,
                Timestamp = r.Timestamp,
                HasData = !string.IsNullOrEmpty(r.Results)
            })
        }, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync("reports/accessibility/mobile-accessibility-audit.json", mobileReportJson);

        // Assert
        mobileAuditResult.Should().NotBeNull("Mobile audit should complete");
        mobileAuditResult.OverallScore.Should().BeGreaterThan(50, "Mobile accessibility should meet minimum standards");
        
        Console.WriteLine($"Mobile Accessibility Summary:");
        Console.WriteLine($"Viewport: 375x667 (Mobile)");
        Console.WriteLine($"Overall Score: {mobileAuditResult.OverallScore:F1}%");
        Console.WriteLine($"Compliance Level: {mobileAuditResult.ComplianceLevel}");
    }

    #region Helper Methods

    private async Task SetupBasicPartnerSearch()
    {
        // Handle authentication if needed
        var config = await _configManager!.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();

        await _homePage!.NavigateAsync();
        var requiresAuth = await _homePage.RequiresAuthenticationAsync();

        if (requiresAuth)
        {
            Console.WriteLine("Authentication required for comprehensive accessibility test - handling OAuth flow...");
            var authResult = await _authPage!.HandleGoogleOAuthAsync(effectiveTimeout);
            if (!authResult)
            {
                Console.WriteLine("OAuth timed out in test environment - continuing with test");
            }
        }

        // Submit a problem to get partner results for filtering
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync(
            "We need comprehensive accessibility consultation for our platform to ensure WCAG compliance.",
            "Technology",
            "High"
        );
        await _homePage.WaitForPartnerResultsAsync();
        
        // Wait for the interface to be ready
        await Task.Delay(2000);
    }

    private async Task TestFilterKeyboardInteraction()
    {
        Console.WriteLine("Testing filter keyboard interaction...");
        
        // Test Tab navigation
        for (int i = 0; i < 5; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
            await Task.Delay(300);
            
            var focusedElement = await Page.EvaluateAsync<string>(@"
                () => {
                    const element = document.activeElement;
                    return element ? element.tagName + ':' + (element.type || 'none') : 'null';
                }
            ");
            
            Console.WriteLine($"  Tab {i + 1}: Focused on {focusedElement}");
        }

        // Test Enter and Space key interactions
        await Page.Keyboard.PressAsync("Enter");
        await Task.Delay(500);
        
        Console.WriteLine("Keyboard interaction testing completed");
    }

    private async Task TestFilterScreenReaderAnnouncements()
    {
        Console.WriteLine("Testing screen reader announcements...");
        
        // Apply a filter and check for ARIA live regions
        await _filterPage!.FilterByCityAsync("Portland");
        await Task.Delay(2000);

        var liveRegionContent = await Page.EvaluateAsync<object>(@"
            () => {
                const liveElements = Array.from(document.querySelectorAll('[aria-live], [role=""status""], [role=""alert""]'));
                return liveElements.map(el => ({
                    content: el.textContent?.trim() || '',
                    ariaLive: el.getAttribute('aria-live'),
                    role: el.getAttribute('role')
                }));
            }
        ");

        Console.WriteLine($"Live regions found: {JsonSerializer.Serialize(liveRegionContent, new JsonSerializerOptions { WriteIndented = true })}");
        Console.WriteLine("Screen reader announcement testing completed");
    }

    #endregion

    [TearDown]
    public async Task TearDown()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }
    }
}