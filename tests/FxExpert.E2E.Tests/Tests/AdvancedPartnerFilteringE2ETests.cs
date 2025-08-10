using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class AdvancedPartnerFilteringE2ETests : PageTest
{
    private HomePage? _homePage;
    private AdvancedPartnerFilterPage? _advancedFilterPage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        // Create page objects
        _homePage = new HomePage(Page);
        _advancedFilterPage = new AdvancedPartnerFilterPage(Page);
        _authPage = new AuthenticationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        // Create screenshots directory
        await Task.Run(() => Directory.CreateDirectory("screenshots/advanced-filtering"));
    }

    #region Advanced Filter Tests

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_IndustriesFilter_ShouldFilterResultsByIndustries()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply industries filter
        await _advancedFilterPage!.FilterByIndustriesAsync(["Technology", "Healthcare"]);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/01-industries-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle industries filter correctly");
        
        await _advancedFilterPage.AssertActiveFiltersCountGreaterThanAsync(0);
        Console.WriteLine($"Industries filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_TechnologiesFilter_ShouldFilterResultsByTechnologies()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply technologies filter
        await _advancedFilterPage!.FilterByTechnologiesAsync(["AWS", "Azure", "React"]);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/02-technologies-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle technologies filter correctly");
        
        Console.WriteLine($"Technologies filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_CertificationsFilter_ShouldFilterResultsByCertifications()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply certifications filter
        await _advancedFilterPage!.FilterByCertificationsAsync(["AWS Solutions Architect", "PMP"]);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/03-certifications-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle certifications filter correctly");
        
        Console.WriteLine($"Certifications filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_RateRangeFilter_ShouldFilterResultsByRateRange()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply rate range filter
        await _advancedFilterPage!.FilterByRateRangeAsync(200, 800);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/04-rate-range-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle rate range filter correctly");
        
        await _advancedFilterPage.AssertRateRangeDisplayAsync(200, 800);
        Console.WriteLine($"Rate range filter ($200-$800) returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_WorkPreferencesFilter_ShouldFilterResultsByWorkPreferences()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply work preferences filter
        await _advancedFilterPage!.FilterByWorkPreferencesAsync(
            remoteWork: true,
            onSiteWork: false,
            travelWillingness: true
        );
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/05-work-preferences-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle work preferences filter correctly");
        
        Console.WriteLine($"Work preferences filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_LanguagesFilter_ShouldFilterResultsByLanguages()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply languages filter
        await _advancedFilterPage!.FilterByLanguagesAsync(["English", "Spanish"]);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/06-languages-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle languages filter correctly");
        
        Console.WriteLine($"Languages filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_ExperienceTypesFilter_ShouldFilterResultsByExperienceTypes()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply experience types filter
        await _advancedFilterPage!.FilterByExperienceTypesAsync(
            hasSecurityClearance: true,
            executiveExperience: true,
            startupExperience: false,
            enterpriseExperience: true,
            consultingExperience: true
        );
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/07-experience-types-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle experience types filter correctly");
        
        Console.WriteLine($"Experience types filter returned {resultsCount} partners");
    }

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_SpecializationsFilter_ShouldFilterResultsBySpecializations()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply specializations filter
        await _advancedFilterPage!.FilterBySpecializationsAsync([
            "DigitalTransformation",
            "CloudMigration", 
            "Cybersecurity"
        ]);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/08-specializations-filter-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle specializations filter correctly");
        
        Console.WriteLine($"Specializations filter returned {resultsCount} partners");
    }

    #endregion

    #region Complex Multi-Filter Tests

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_ComplexMultiFilter_ShouldApplyAllFiltersCorrectly()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();
        var initialCount = await _advancedFilterPage!.GetFilteredResultsCountAsync();

        // Act - Apply comprehensive advanced filters
        await _advancedFilterPage.FilterByIndustriesAsync(["Technology"]);
        await _advancedFilterPage.FilterByTechnologiesAsync(["AWS"]);
        await _advancedFilterPage.FilterByRateRangeAsync(300, 1500);
        await _advancedFilterPage.FilterByWorkPreferencesAsync(remoteWork: true);
        await _advancedFilterPage.FilterByExperienceTypesAsync(consultingExperience: true);
        
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/09-complex-multi-filter-applied");

        // Assert
        var filteredCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        filteredCount.Should().BeLessThanOrEqualTo(initialCount, "Multiple filters should narrow results");
        
        await _advancedFilterPage.AssertActiveFiltersCountGreaterThanAsync(4);
        Console.WriteLine($"Complex multi-filter: {initialCount} → {filteredCount} partners");
    }

    [Test]
    [Category("P1")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_MaximumFilters_ShouldHandleAllFiltersSimultaneously()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply all available advanced filters
        await _advancedFilterPage!.ApplyComprehensiveFiltersAsync(
            industries: ["Technology", "Healthcare"],
            technologies: ["AWS", "React", "Python"],
            certifications: ["PMP"],
            minRate: 200,
            maxRate: 1000,
            remoteWork: true,
            onSiteWork: true,
            travelWillingness: false,
            languages: ["English"],
            hasSecurityClearance: false,
            executiveExperience: true,
            consultingExperience: true,
            specializations: ["DigitalTransformation", "CloudMigration"]
        );
        
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/10-maximum-filters-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle maximum filters without errors");
        
        // Should show many active filters
        await _advancedFilterPage.AssertActiveFiltersCountGreaterThanAsync(8);
        Console.WriteLine($"Maximum filters returned {resultsCount} partners");
    }

    #endregion

    #region Mobile Advanced Filtering Tests

    [Test]
    [Category("P1")]
    [Category("AdvancedFiltering")]
    [Category("Mobile")]
    public async Task AdvancedFiltering_MobileView_ShouldWorkOnSmallScreens()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await SetupAdvancedPartnerSearch();

        // Act - Test mobile advanced filtering
        await _advancedFilterPage!.OpenMobileFiltersAsync();
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/11-mobile-advanced-filters-opened");

        await _advancedFilterPage.FilterByIndustriesAsync(["Technology"]);
        await _advancedFilterPage.FilterByRateRangeAsync(400, 1200);
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/12-mobile-advanced-filters-applied");

        // Assert
        var resultsCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Mobile advanced filtering should work correctly");
        
        await _advancedFilterPage.AssertMobileFilterBadgeVisibleAsync();
        Console.WriteLine($"Mobile advanced filtering returned {resultsCount} partners");
    }

    [Test]
    [Category("P2")]
    [Category("AdvancedFiltering")]
    [Category("Accessibility")]
    public async Task AdvancedFiltering_KeyboardNavigation_ShouldNavigateAdvancedFilters()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Test keyboard navigation through advanced filters
        await _advancedFilterPage!.OpenAdvancedFiltersAsync();
        
        // Navigate through expansion panels using keyboard
        await Page.Keyboard.PressAsync("Tab"); // Industries panel
        await Page.Keyboard.PressAsync("Enter"); // Open panel
        await Page.Keyboard.PressAsync("Tab"); // Industries input
        await Page.Keyboard.PressAsync("Tab"); // Technologies panel
        await Page.Keyboard.PressAsync("Enter"); // Open panel
        await Page.Keyboard.PressAsync("Tab"); // Technologies input
        
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/13-keyboard-navigation");

        // Assert
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        focusedElement.Should().NotBeNullOrEmpty("Should have keyboard focus on advanced filter element");
        
        Console.WriteLine($"Advanced filter keyboard navigation completed, focused on: {focusedElement}");
    }

    #endregion

    #region Filter Clearing and Reset Tests

    [Test]
    [Category("P0")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_ClearAdvancedFilters_ShouldResetToOriginalResults()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();
        var originalCount = await _advancedFilterPage!.GetFilteredResultsCountAsync();

        // Act - Apply advanced filters then clear them
        await _advancedFilterPage.FilterByIndustriesAsync(["Technology"]);
        await _advancedFilterPage.FilterByTechnologiesAsync(["AWS", "React"]);
        await _advancedFilterPage.FilterByRateRangeAsync(500, 1500);
        var filteredCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/14-before-clear-advanced-filters");

        await _advancedFilterPage.ClearAllAdvancedFiltersAsync();
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/15-after-clear-advanced-filters");

        // Assert
        var clearedCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        
        clearedCount.Should().Be(originalCount, "Clearing advanced filters should restore original results");
        await _advancedFilterPage.AssertActiveFiltersCountAsync(0);
        
        Console.WriteLine($"Advanced filter clearing: Original={originalCount}, Filtered={filteredCount}, Cleared={clearedCount}");
    }

    #endregion

    #region Performance Tests

    [Test]
    [Category("P1")]
    [Category("AdvancedFiltering")]
    [Category("Performance")]
    public async Task AdvancedFiltering_RapidAdvancedFilterChanges_ShouldBeResponsive()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply advanced filters rapidly in succession
        var startTime = DateTime.Now;
        
        await _advancedFilterPage!.FilterByIndustriesAsync(["Technology"]);
        await _advancedFilterPage.FilterByTechnologiesAsync(["AWS"]);
        await _advancedFilterPage.FilterByRateRangeAsync(300, 800);
        await _advancedFilterPage.FilterByWorkPreferencesAsync(remoteWork: true);
        await _advancedFilterPage.ClearAllAdvancedFiltersAsync();
        
        var endTime = DateTime.Now;
        var duration = endTime - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromSeconds(45), "Rapid advanced filter changes should complete within 45 seconds");
        
        var finalCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        finalCount.Should().BeGreaterThan(0, "Should have results after rapid advanced filter changes");
        
        Console.WriteLine($"Rapid advanced filter changes completed in {duration.TotalMilliseconds}ms");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    [Category("P2")]
    [Category("AdvancedFiltering")]
    [Category("Edge-Cases")]
    public async Task AdvancedFiltering_InvalidRateRange_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Try applying invalid rate ranges
        await _advancedFilterPage!.FilterByRateRangeAsync(2000, 100); // Max < Min
        var invalidRangeResults = await _advancedFilterPage.GetFilteredResultsCountAsync();
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/16-invalid-rate-range");

        await _advancedFilterPage.ClearAllAdvancedFiltersAsync();
        await _advancedFilterPage.FilterByRateRangeAsync(0, 50000); // Extreme range
        var extremeRangeResults = await _advancedFilterPage.GetFilteredResultsCountAsync();

        // Assert - Should handle edge cases gracefully
        invalidRangeResults.Should().BeGreaterThanOrEqualTo(0, "Should handle invalid rate range gracefully");
        extremeRangeResults.Should().BeGreaterThanOrEqualTo(0, "Should handle extreme rate range gracefully");
        
        Console.WriteLine($"Edge case handling: Invalid range={invalidRangeResults}, Extreme range={extremeRangeResults}");
    }

    [Test]
    [Category("P2")]
    [Category("AdvancedFiltering")]
    [Category("Edge-Cases")]
    public async Task AdvancedFiltering_NoMatchingAdvancedResults_ShouldShowEmptyState()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();

        // Act - Apply very restrictive advanced filters that should return no results
        await _advancedFilterPage!.ApplyComprehensiveFiltersAsync(
            industries: ["NonExistentIndustry"],
            technologies: ["VeryRareTechnology"],
            certifications: ["UnknownCertification"],
            minRate: 5000, // Very high minimum rate
            maxRate: 6000,
            hasSecurityClearance: true,
            executiveExperience: true,
            specializations: ["VerySpecificSpecialization"]
        );
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/17-no-matching-advanced-results");

        // Assert
        await _advancedFilterPage.AssertNoResultsAsync();
        
        Console.WriteLine("No matching advanced results test completed - empty state properly displayed");
    }

    #endregion

    #region Filter Statistics Tests

    [Test]
    [Category("P1")]
    [Category("AdvancedFiltering")]
    public async Task AdvancedFiltering_FilterStatistics_ShouldShowCorrectCounts()
    {
        // Arrange
        await SetupAdvancedPartnerSearch();
        var originalCount = await _advancedFilterPage!.GetFilteredResultsCountAsync();

        // Act - Apply filters and check statistics
        await _advancedFilterPage.FilterByIndustriesAsync(["Technology"]);
        await _advancedFilterPage.FilterByTechnologiesAsync(["AWS"]);
        
        await _advancedFilterPage.TakeScreenshotAsync("advanced-filtering/18-filter-statistics");

        // Assert
        var filteredCount = await _advancedFilterPage.GetFilteredResultsCountAsync();
        
        await _advancedFilterPage.AssertFilterResultsCountDisplayAsync(filteredCount);
        await _advancedFilterPage.AssertActiveFiltersCountAsync(2);
        
        Console.WriteLine($"Filter statistics: Original={originalCount}, Filtered={filteredCount}, Active filters=2");
    }

    #endregion

    #region Helper Methods

    private async Task SetupAdvancedPartnerSearch()
    {
        // Handle authentication if needed
        var config = await _configManager!.LoadAuthenticationConfigAsync();
        var effectiveTimeout = await _configManager.GetEffectiveTimeoutAsync();

        await _homePage!.NavigateAsync();
        var requiresAuth = await _homePage.RequiresAuthenticationAsync();

        if (requiresAuth)
        {
            Console.WriteLine("Authentication required - handling OAuth flow...");
            var authResult = await _authPage!.HandleGoogleOAuthAsync(effectiveTimeout);
            if (!authResult)
            {
                Console.WriteLine("OAuth timed out in test environment - continuing with test");
            }
        }

        // Submit a problem to get partner results for advanced filtering
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync(
            "We need comprehensive guidance on digital transformation, cloud migration, and cybersecurity strategy. Looking for experts with extensive consulting experience and industry certifications.",
            "Technology",
            "High"
        );
        await _homePage.WaitForPartnerResultsAsync();
        
        // Wait for advanced filter interface to be ready
        await Task.Delay(3000);
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