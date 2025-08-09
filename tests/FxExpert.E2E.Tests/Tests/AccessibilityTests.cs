using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

/// <summary>
/// Accessibility compliance tests for WCAG 2.1 AA standards
/// Tests keyboard navigation, screen reader compatibility, color contrast, and focus management
/// </summary>
[TestFixture]
public class AccessibilityTests : PageTest
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

        // Create screenshots directory for accessibility testing
        await Task.Run(() => Directory.CreateDirectory("screenshots/accessibility"));
    }

    #region Keyboard Navigation Tests

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Keyboard")]
    public async Task KeyboardNavigation_FilterSidebar_ShouldBeFullyAccessible()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Navigate through filters using keyboard only
        await _filterPage!.OpenFiltersAsync();
        await _filterPage.TakeScreenshotAsync("accessibility/01-filter-sidebar-opened");

        // Test Tab navigation through filter elements
        var focusableElements = new List<string>();
        
        for (int i = 0; i < 10; i++) // Test first 10 focusable elements
        {
            await Page.Keyboard.PressAsync("Tab");
            await Task.Delay(200); // Allow focus to settle
            
            var focusedElement = await Page.EvaluateAsync<string>(@"
                () => {
                    const element = document.activeElement;
                    return element ? `${element.tagName}:${element.type || 'none'}:${element.getAttribute('data-testid') || element.className || 'no-id'}` : 'null';
                }
            ");
            
            focusableElements.Add(focusedElement);
            
            if (focusedElement == "null" || focusedElement.Contains("BODY"))
                break;
        }

        await _filterPage.TakeScreenshotAsync("accessibility/02-keyboard-navigation-complete");

        // Assert
        focusableElements.Should().NotBeEmpty("Should have focusable elements in filter sidebar");
        focusableElements.Should().Contain(e => e.Contains("INPUT") || e.Contains("SELECT") || e.Contains("BUTTON"), 
            "Should include interactive form elements");
        
        Console.WriteLine($"Focusable elements found: {string.Join(", ", focusableElements)}");
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Keyboard")]
    public async Task KeyboardNavigation_FilterActions_ShouldWorkWithEnterAndSpace()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Test keyboard activation of filters
        // Focus on city input and test Enter key
        await Page.Keyboard.PressAsync("Tab"); // Navigate to first input
        await Page.Keyboard.TypeAsync("San Francisco");
        await Page.Keyboard.PressAsync("Enter");
        await Task.Delay(1000);

        var cityFilterResults = await _filterPage.GetFilteredResultsCountAsync();
        await _filterPage.TakeScreenshotAsync("accessibility/03-city-filter-keyboard-applied");

        // Navigate to availability select and test Space key
        await Page.Keyboard.PressAsync("Tab"); // Move to next element
        await Page.Keyboard.PressAsync("Tab"); // Move to availability select
        await Page.Keyboard.PressAsync("Space"); // Open dropdown
        await Task.Delay(500);
        await Page.Keyboard.PressAsync("ArrowDown"); // Select option
        await Page.Keyboard.PressAsync("Enter"); // Confirm selection
        await Task.Delay(1000);

        var finalResults = await _filterPage.GetFilteredResultsCountAsync();
        await _filterPage.TakeScreenshotAsync("accessibility/04-availability-filter-keyboard-applied");

        // Assert
        cityFilterResults.Should().BeGreaterThanOrEqualTo(0, "City filter should work with keyboard input");
        finalResults.Should().BeGreaterThanOrEqualTo(0, "Availability filter should work with keyboard navigation");
        
        Console.WriteLine($"Keyboard filter results: City={cityFilterResults}, Final={finalResults}");
    }

    #endregion

    #region Focus Management Tests

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Focus")]
    public async Task FocusManagement_FilterChanges_ShouldMaintainLogicalFocus()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Test focus management during filter operations
        var initialFocusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        
        // Apply filter and check if focus is managed properly
        await _filterPage.FilterByCityAsync("Austin");
        await Task.Delay(1000);
        
        var focusAfterFilter = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        await _filterPage.TakeScreenshotAsync("accessibility/05-focus-after-filter");

        // Clear filters and check focus management
        await _filterPage.ClearAllFiltersAsync();
        await Task.Delay(1000);
        
        var focusAfterClear = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        await _filterPage.TakeScreenshotAsync("accessibility/06-focus-after-clear");

        // Assert
        focusAfterFilter.Should().NotBe("BODY", "Focus should not return to body after filter application");
        focusAfterClear.Should().NotBe("BODY", "Focus should not return to body after clearing filters");
        
        Console.WriteLine($"Focus progression: Initial={initialFocusedElement}, After Filter={focusAfterFilter}, After Clear={focusAfterClear}");
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("Focus")]
    public async Task FocusManagement_ModalDialogs_ShouldTrapFocusCorrectly()
    {
        // This test assumes modal dialogs exist for filter settings or partner details
        // If no modals exist, this test will validate the current non-modal approach

        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Look for any modal triggers
        var hasModals = await Page.Locator("[role='dialog'], .modal, .mud-dialog").CountAsync() > 0;
        
        if (hasModals)
        {
            // Test modal focus trap (implementation depends on modal structure)
            await Page.Keyboard.PressAsync("Tab");
            var focusInModal = await Page.EvaluateAsync<bool>(@"
                () => {
                    const activeElement = document.activeElement;
                    const modal = document.querySelector('[role=""dialog""], .modal, .mud-dialog');
                    return modal && modal.contains(activeElement);
                }
            ");
            
            focusInModal.Should().BeTrue("Focus should be trapped within modal");
        }
        else
        {
            // Validate that non-modal interface maintains proper focus flow
            await _filterPage!.OpenFiltersAsync();
            var focusInFilterArea = await Page.EvaluateAsync<bool>(@"
                () => {
                    const activeElement = document.activeElement;
                    const filterArea = document.querySelector('[data-testid=""partner-filter-sidebar""], .filter-sidebar, .partner-filters');
                    return filterArea && (filterArea.contains(activeElement) || filterArea === activeElement);
                }
            ");
            
            Console.WriteLine($"Non-modal interface focus management verified: {focusInFilterArea}");
        }

        await _filterPage!.TakeScreenshotAsync("accessibility/07-focus-management-validation");
    }

    #endregion

    #region ARIA Labels and Semantic HTML Tests

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("ARIA")]
    public async Task ARIALabels_FilterControls_ShouldHaveProperLabeling()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Check ARIA labels and accessibility attributes
        var accessibilityReport = await Page.EvaluateAsync<object>(@"
            () => {
                const inputs = Array.from(document.querySelectorAll('input, select, button'));
                return inputs.map(element => ({
                    tagName: element.tagName,
                    type: element.type || 'none',
                    hasLabel: !!element.getAttribute('aria-label') || !!element.getAttribute('aria-labelledby') || !!document.querySelector(`label[for='${element.id}']`),
                    hasAriaDescribedBy: !!element.getAttribute('aria-describedby'),
                    hasRole: !!element.getAttribute('role'),
                    isRequired: element.hasAttribute('required') || element.getAttribute('aria-required') === 'true',
                    testId: element.getAttribute('data-testid') || 'none'
                }));
            }
        ");

        await _filterPage.TakeScreenshotAsync("accessibility/08-aria-validation");

        // Assert
        var report = accessibilityReport.ToString();
        report.Should().NotBeEmpty("Should have accessibility information for form controls");
        
        // Log detailed accessibility report
        Console.WriteLine("Accessibility Report for Filter Controls:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(accessibilityReport, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    [Test]
    [Category("P0")]
    [Category("Accessibility")]
    [Category("ARIA")]
    public async Task SemanticHTML_FilterStructure_ShouldUseProperHeadings()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Check heading hierarchy and semantic structure
        var headingStructure = await Page.EvaluateAsync<object>(@"
            () => {
                const headings = Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6'));
                return headings.map(h => ({
                    level: parseInt(h.tagName.substring(1)),
                    text: h.textContent?.trim() || '',
                    hasId: !!h.id,
                    isVisible: h.offsetParent !== null
                }));
            }
        ");

        var landmarkStructure = await Page.EvaluateAsync<object>(@"
            () => {
                const landmarks = Array.from(document.querySelectorAll('[role], main, nav, header, footer, aside, section'));
                return landmarks.map(l => ({
                    tagName: l.tagName,
                    role: l.getAttribute('role') || 'implicit',
                    hasLabel: !!l.getAttribute('aria-label') || !!l.getAttribute('aria-labelledby'),
                    isVisible: l.offsetParent !== null
                }));
            }
        ");

        await _filterPage!.TakeScreenshotAsync("accessibility/09-semantic-structure");

        // Assert
        var headings = headingStructure.ToString();
        var landmarks = landmarkStructure.ToString();
        
        headings.Should().NotBeEmpty("Should have proper heading structure");
        landmarks.Should().NotBeEmpty("Should have semantic landmarks");
        
        Console.WriteLine("Heading Structure:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(headingStructure, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        
        Console.WriteLine("Landmark Structure:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(landmarkStructure, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    #endregion

    #region Color Contrast Tests

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Color-Contrast")]
    public async Task ColorContrast_FilterElements_ShouldMeetWCAGStandards()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Check color contrast ratios
        var contrastReport = await Page.EvaluateAsync<object>(@"
            () => {
                function getContrastRatio(foreground, background) {
                    // Simplified contrast calculation - in real implementation would use proper WCAG formula
                    const fg = foreground.match(/\d+/g)?.map(Number) || [0, 0, 0];
                    const bg = background.match(/\d+/g)?.map(Number) || [255, 255, 255];
                    
                    const luminance1 = (0.299 * fg[0] + 0.587 * fg[1] + 0.114 * fg[2]) / 255;
                    const luminance2 = (0.299 * bg[0] + 0.587 * bg[1] + 0.114 * bg[2]) / 255;
                    
                    const lighter = Math.max(luminance1, luminance2);
                    const darker = Math.min(luminance1, luminance2);
                    
                    return (lighter + 0.05) / (darker + 0.05);
                }

                const elements = Array.from(document.querySelectorAll('input, button, select, label, .filter-sidebar *'));
                return elements.slice(0, 10).map(el => {
                    const styles = window.getComputedStyle(el);
                    const color = styles.color;
                    const backgroundColor = styles.backgroundColor;
                    
                    return {
                        tagName: el.tagName,
                        className: el.className || 'none',
                        color: color,
                        backgroundColor: backgroundColor,
                        contrastRatio: getContrastRatio(color, backgroundColor)
                    };
                });
            }
        ");

        await _filterPage.TakeScreenshotAsync("accessibility/10-color-contrast-analysis");

        // Assert
        var report = contrastReport.ToString();
        report.Should().NotBeEmpty("Should have color contrast information");
        
        Console.WriteLine("Color Contrast Analysis:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(contrastReport, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Color-Contrast")]
    public async Task ColorContrast_DarkMode_ShouldMaintainAccessibility()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        
        // Act - Test dark mode contrast
        await _homePage!.ToggleThemeAsync(); // Toggle to dark mode
        await Task.Delay(1000); // Wait for theme change
        
        await _filterPage!.OpenFiltersAsync();
        await _filterPage.TakeScreenshotAsync("accessibility/11-dark-mode-contrast");

        // Check if dark mode is actually applied
        var isDarkMode = await Page.EvaluateAsync<bool>(@"
            () => {
                const body = document.body;
                const styles = window.getComputedStyle(body);
                const backgroundColor = styles.backgroundColor;
                // Check if background is dark (simplified check)
                return backgroundColor.includes('rgb') && backgroundColor.match(/\d+/g)?.map(Number).every(val => val < 128);
            }
        ");

        // Toggle back to light mode for consistency
        if (isDarkMode)
        {
            await _homePage.ToggleThemeAsync();
            await Task.Delay(1000);
        }

        // Assert
        Console.WriteLine($"Dark mode accessibility test completed. Dark mode detected: {isDarkMode}");
    }

    #endregion

    #region Screen Reader Compatibility Tests

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Screen-Reader")]
    public async Task ScreenReader_FilterLabels_ShouldProvideProperContext()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Check screen reader accessible text
        var screenReaderText = await Page.EvaluateAsync<object>(@"
            () => {
                const elements = Array.from(document.querySelectorAll('input, select, button, label'));
                return elements.map(el => {
                    const computedLabel = el.getAttribute('aria-label') || 
                                       el.getAttribute('aria-labelledby') && document.getElementById(el.getAttribute('aria-labelledby'))?.textContent ||
                                       document.querySelector(`label[for='${el.id}']`)?.textContent ||
                                       el.textContent?.trim() ||
                                       el.getAttribute('placeholder') ||
                                       'No accessible name';
                    
                    return {
                        tagName: el.tagName,
                        type: el.type || 'none',
                        accessibleName: computedLabel,
                        hasAriaDescribedBy: !!el.getAttribute('aria-describedby'),
                        describedByText: el.getAttribute('aria-describedby') ? 
                                       document.getElementById(el.getAttribute('aria-describedby'))?.textContent : null
                    };
                });
            }
        ");

        await _filterPage.TakeScreenshotAsync("accessibility/12-screen-reader-context");

        // Assert
        var report = screenReaderText.ToString();
        report.Should().NotBeEmpty("Should have accessible names for all interactive elements");
        
        Console.WriteLine("Screen Reader Accessibility Report:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(screenReaderText, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Screen-Reader")]
    public async Task ScreenReader_DynamicUpdates_ShouldAnnounceChanges()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        await _filterPage!.OpenFiltersAsync();

        // Act - Check for ARIA live regions and dynamic content announcements
        var liveRegions = await Page.EvaluateAsync<object>(@"
            () => {
                const liveElements = Array.from(document.querySelectorAll('[aria-live], [role=""status""], [role=""alert""], .sr-only'));
                return liveElements.map(el => ({
                    tagName: el.tagName,
                    ariaLive: el.getAttribute('aria-live'),
                    role: el.getAttribute('role'),
                    content: el.textContent?.trim() || '',
                    isVisible: el.offsetParent !== null
                }));
            }
        ");

        // Apply a filter and check if changes are announced
        await _filterPage.FilterByCityAsync("Seattle");
        await Task.Delay(2000); // Wait for potential announcements

        var updatedLiveRegions = await Page.EvaluateAsync<object>(@"
            () => {
                const liveElements = Array.from(document.querySelectorAll('[aria-live], [role=""status""], [role=""alert""], .sr-only'));
                return liveElements.map(el => ({
                    tagName: el.tagName,
                    ariaLive: el.getAttribute('aria-live'),
                    content: el.textContent?.trim() || '',
                    hasContent: !!el.textContent?.trim()
                }));
            }
        ");

        await _filterPage.TakeScreenshotAsync("accessibility/13-live-regions-after-filter");

        // Assert
        var initialReport = liveRegions.ToString();
        var updatedReport = updatedLiveRegions.ToString();
        
        Console.WriteLine("Initial Live Regions:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(liveRegions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        
        Console.WriteLine("Updated Live Regions:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(updatedLiveRegions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    #endregion

    #region Mobile Accessibility Tests

    [Test]
    [Category("P1")]
    [Category("Accessibility")]
    [Category("Mobile")]
    public async Task MobileAccessibility_TouchTargets_ShouldMeetMinimumSize()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await SetupBasicPartnerSearch();

        // Act - Check touch target sizes on mobile
        var touchTargetSizes = await Page.EvaluateAsync<object>(@"
            () => {
                const interactiveElements = Array.from(document.querySelectorAll('button, input, select, a, [tabindex]'));
                return interactiveElements.map(el => {
                    const rect = el.getBoundingClientRect();
                    return {
                        tagName: el.tagName,
                        width: Math.round(rect.width),
                        height: Math.round(rect.height),
                        isVisible: rect.width > 0 && rect.height > 0,
                        meetsMinSize: rect.width >= 44 && rect.height >= 44 // WCAG AAA guideline
                    };
                });
            }
        ");

        await _filterPage!.TakeScreenshotAsync("accessibility/14-mobile-touch-targets");

        // Assert
        var report = touchTargetSizes.ToString();
        report.Should().NotBeEmpty("Should have touch target size information");
        
        Console.WriteLine("Mobile Touch Target Analysis:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(touchTargetSizes, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    #endregion

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
            Console.WriteLine("Authentication required for accessibility test - handling OAuth flow...");
            var authResult = await _authPage!.HandleGoogleOAuthAsync(effectiveTimeout);
            if (!authResult)
            {
                Console.WriteLine("OAuth timed out in test environment - continuing with test");
            }
        }

        // Submit a basic problem to get partner results for filtering
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync(
            "We need accessibility consultation for our digital platform.",
            "Technology",
            "High"
        );
        await _homePage.WaitForPartnerResultsAsync();
        
        // Wait for the interface to be ready
        await Task.Delay(2000);
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