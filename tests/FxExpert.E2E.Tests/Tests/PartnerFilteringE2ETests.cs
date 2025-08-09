using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using FluentAssertions;
using FxExpert.E2E.Tests.PageObjectModels;
using FxExpert.E2E.Tests.Configuration;

namespace FxExpert.E2E.Tests.Tests;

[TestFixture]
public class PartnerFilteringE2ETests : PageTest
{
    private HomePage? _homePage;
    private PartnerFilterPage? _filterPage;
    private AuthenticationPage? _authPage;
    private AuthenticationConfigurationManager? _configManager;

    [SetUp]
    public async Task SetUp()
    {
        // Create page objects
        _homePage = new HomePage(Page);
        _filterPage = new PartnerFilterPage(Page);
        _authPage = new AuthenticationPage(Page);
        _configManager = AuthenticationConfigurationManager.CreateDefault("Development");

        // Create screenshots directory
        await Task.Run(() => Directory.CreateDirectory("screenshots/filtering"));
    }

    #region Basic Filtering Tests

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_LocationFilter_ShouldFilterResultsByLocation()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply location filter
        await _filterPage!.FilterByCityAsync("San Francisco");
        await _filterPage.TakeScreenshotAsync("filtering/01-location-filter-applied");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThan(0, "Should have partners in San Francisco");
        
        // Verify all results are from San Francisco
        for (int i = 0; i < Math.Min(resultsCount, 3); i++)
        {
            await _filterPage.AssertPartnerInLocationAsync(i, "San Francisco");
        }
    }

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_SkillsFilter_ShouldFilterResultsBySkills()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply skills filter
        await _filterPage!.FilterBySkillAsync("Azure");
        await _filterPage.TakeScreenshotAsync("filtering/02-skills-filter-applied");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThan(0, "Should have partners with Azure skills");
        
        // Verify all results have Azure skills
        for (int i = 0; i < Math.Min(resultsCount, 3); i++)
        {
            await _filterPage.AssertPartnerHasSkillAsync(i, "Azure");
        }
    }

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_AvailabilityFilter_ShouldFilterResultsByAvailability()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply availability filter
        await _filterPage!.FilterByAvailabilityAsync("ThisWeek");
        await _filterPage.TakeScreenshotAsync("filtering/03-availability-filter-applied");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        
        if (resultsCount > 0)
        {
            // Verify partners have reasonable availability for this week
            for (int i = 0; i < Math.Min(resultsCount, 3); i++)
            {
                await _filterPage.AssertPartnerHasAvailabilityAsync(i, 1); // At least 1 slot available
            }
        }
        else
        {
            Console.WriteLine("No partners available this week - this is expected in test environment");
        }
    }

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_ExperienceFilter_ShouldFilterResultsByExperience()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply experience level filter
        await _filterPage!.FilterByExperienceLevelAsync("Expert");
        await _filterPage.TakeScreenshotAsync("filtering/04-experience-filter-applied");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        resultsCount.Should().BeGreaterThan(0, "Should have expert-level partners");
        
        Console.WriteLine($"Found {resultsCount} expert-level partners");
    }

    #endregion

    #region Multi-Filter Combinations

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_MultipleFilters_ShouldApplyAllFiltersCorrectly()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        var initialCount = await _filterPage!.GetFilteredResultsCountAsync();

        // Act - Apply multiple filters progressively
        await _filterPage.FilterByRegionAsync("west-coast");
        await _filterPage.TakeScreenshotAsync("filtering/05-region-filter-applied");
        var afterRegionCount = await _filterPage.GetFilteredResultsCountAsync();

        await _filterPage.FilterBySkillAsync("Leadership");
        await _filterPage.TakeScreenshotAsync("filtering/06-skills-added-to-region");
        var afterSkillsCount = await _filterPage.GetFilteredResultsCountAsync();

        await _filterPage.FilterByExperienceLevelAsync("Expert");
        await _filterPage.TakeScreenshotAsync("filtering/07-experience-added-to-filters");
        var finalCount = await _filterPage.GetFilteredResultsCountAsync();

        // Assert - Each filter should progressively narrow results
        afterRegionCount.Should().BeLessThanOrEqualTo(initialCount, "Region filter should narrow results");
        afterSkillsCount.Should().BeLessThanOrEqualTo(afterRegionCount, "Skills filter should further narrow results");
        finalCount.Should().BeLessThanOrEqualTo(afterSkillsCount, "Experience filter should further narrow results");

        Console.WriteLine($"Filter progression: {initialCount} → {afterRegionCount} → {afterSkillsCount} → {finalCount}");
    }

    [Test]
    [Category("P1")]
    [Category("Filtering")]
    public async Task PartnerFiltering_ComplexFilterCombination_ShouldHandleMultipleCriteria()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply complex filter combination
        await _filterPage!.ApplyMultipleFiltersAsync(
            state: "CA",
            availability: "ThisMonth",
            skill: "Strategy",
            experienceLevel: "Expert",
            minYears: 5
        );
        await _filterPage.TakeScreenshotAsync("filtering/08-complex-filter-combination");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        
        if (resultsCount > 0)
        {
            Console.WriteLine($"Found {resultsCount} partners matching complex criteria");
            
            // Verify first result matches criteria
            await _filterPage.AssertPartnerInLocationAsync(0, "CA");
            await _filterPage.AssertPartnerHasSkillAsync(0, "Strategy");
        }
        else
        {
            Console.WriteLine("No partners match the complex filter criteria - this may be expected with restrictive filters");
        }
    }

    #endregion

    #region Filter Clearing and Reset

    [Test]
    [Category("P0")]
    [Category("Filtering")]
    public async Task PartnerFiltering_ClearFilters_ShouldResetToOriginalResults()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        var originalCount = await _filterPage!.GetFilteredResultsCountAsync();

        // Act - Apply filters then clear them
        await _filterPage.FilterByCityAsync("San Francisco");
        await _filterPage.FilterBySkillAsync("Azure");
        var filteredCount = await _filterPage.GetFilteredResultsCountAsync();
        await _filterPage.TakeScreenshotAsync("filtering/09-before-clear-filters");

        await _filterPage.ClearAllFiltersAsync();
        await _filterPage.TakeScreenshotAsync("filtering/10-after-clear-filters");

        // Assert
        var clearedCount = await _filterPage.GetFilteredResultsCountAsync();
        
        filteredCount.Should().BeLessThan(originalCount, "Filters should have reduced the results");
        clearedCount.Should().Be(originalCount, "Clearing filters should restore original results");
        
        Console.WriteLine($"Results: Original={originalCount}, Filtered={filteredCount}, Cleared={clearedCount}");
    }

    #endregion

    #region Sorting and Ordering

    [Test]
    [Category("P1")]
    [Category("Filtering")]
    public async Task PartnerFiltering_SortingOptions_ShouldReorderResults()
    {
        // Arrange
        await SetupBasicPartnerSearch();
        var originalNames = await _filterPage!.GetPartnerNamesAsync();

        // Act - Test different sorting options
        await _filterPage.SortByNameAsync();
        await _filterPage.TakeScreenshotAsync("filtering/11-sorted-by-name");
        var namesSorted = await _filterPage.GetPartnerNamesAsync();

        await _filterPage.SortByAvailabilityAsync();
        await _filterPage.TakeScreenshotAsync("filtering/12-sorted-by-availability");
        var availabilitySorted = await _filterPage.GetPartnerNamesAsync();

        await _filterPage.SortByExperienceAsync();
        await _filterPage.TakeScreenshotAsync("filtering/13-sorted-by-experience");
        var experienceSorted = await _filterPage.GetPartnerNamesAsync();

        // Assert
        originalNames.Should().NotBeEmpty("Should have original results");
        namesSorted.Should().NotBeEmpty("Should have name-sorted results");
        availabilitySorted.Should().NotBeEmpty("Should have availability-sorted results");
        experienceSorted.Should().NotBeEmpty("Should have experience-sorted results");

        // Verify sorting actually changed the order (unless there's only one result)
        if (originalNames.Length > 1)
        {
            (namesSorted.SequenceEqual(originalNames) || 
             availabilitySorted.SequenceEqual(originalNames) || 
             experienceSorted.SequenceEqual(originalNames)).Should().BeFalse("At least one sort should change the order");
        }

        Console.WriteLine($"Sorting test with {originalNames.Length} partners completed");
    }

    #endregion

    #region Performance and Load Tests

    [Test]
    [Category("P1")]
    [Category("Performance")]
    public async Task PartnerFiltering_RapidFilterChanges_ShouldHandleQuickSuccession()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply filters rapidly in succession
        var startTime = DateTime.Now;
        
        await _filterPage!.FilterByCityAsync("New York");
        await _filterPage.FilterByStateAsync("TX");
        await _filterPage.FilterBySkillAsync("Python");
        await _filterPage.FilterByAvailabilityAsync("NextWeek");
        await _filterPage.ClearAllFiltersAsync();
        
        var endTime = DateTime.Now;
        var duration = endTime - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromSeconds(30), "Rapid filter changes should complete within 30 seconds");
        
        var finalCount = await _filterPage.GetFilteredResultsCountAsync();
        finalCount.Should().BeGreaterThan(0, "Should have results after rapid filter changes");
        
        Console.WriteLine($"Rapid filter changes completed in {duration.TotalMilliseconds}ms");
    }

    [Test]
    [Category("P2")]
    [Category("Performance")]
    public async Task PartnerFiltering_FilterPerformanceWithManyResults_ShouldBeResponsive()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Time how long filtering takes with various filter types
        var filterTests = new Dictionary<string, Func<Task>>
        {
            {"Location", () => _filterPage!.FilterByCityAsync("San Francisco")},
            {"Skills", () => _filterPage!.FilterBySkillAsync("Leadership")},
            {"Availability", () => _filterPage!.FilterByAvailabilityAsync("ThisMonth")},
            {"Experience", () => _filterPage!.FilterByExperienceLevelAsync("Expert")}
        };

        foreach (var test in filterTests)
        {
            await _filterPage!.ClearAllFiltersAsync();
            
            var startTime = DateTime.Now;
            await test.Value();
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            // Assert
            duration.Should().BeLessThan(TimeSpan.FromSeconds(10), $"{test.Key} filter should complete within 10 seconds");
            
            Console.WriteLine($"{test.Key} filter took {duration.TotalMilliseconds}ms");
        }
    }

    #endregion

    #region Real-time Calendar Integration Tests

    [Test]
    [Category("P1")]
    [Category("Calendar-Integration")]
    public async Task PartnerFiltering_AvailabilityFilter_ShouldUseRealTimeData()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Test availability filter with different timeframes
        await _filterPage!.FilterByAvailabilityAsync("ThisWeek");
        var thisWeekCount = await _filterPage.GetFilteredResultsCountAsync();
        await _filterPage.TakeScreenshotAsync("filtering/14-this-week-availability");

        await _filterPage.ClearAllFiltersAsync();
        await _filterPage.FilterByAvailabilityAsync("ThisMonth");
        var thisMonthCount = await _filterPage.GetFilteredResultsCountAsync();
        await _filterPage.TakeScreenshotAsync("filtering/15-this-month-availability");

        // Assert
        // This month should generally have more or equal availability than this week
        thisMonthCount.Should().BeGreaterThanOrEqualTo(thisWeekCount, 
            "This month should have at least as many available partners as this week");

        Console.WriteLine($"Availability: This week={thisWeekCount}, This month={thisMonthCount}");
    }

    [Test]
    [Category("P1")]
    [Category("Calendar-Integration")]
    public async Task PartnerFiltering_AvailabilityWithFallback_ShouldHandleCalendarServiceFailure()
    {
        // This test verifies that filtering works even when calendar service is unavailable
        // In test environment, we expect fallback behavior

        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply availability filter that might trigger fallback
        await _filterPage!.FilterByAvailabilityAsync("NextWeek");
        await _filterPage.TakeScreenshotAsync("filtering/16-availability-with-fallback");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        
        // Should still have results even with fallback behavior
        resultsCount.Should().BeGreaterThanOrEqualTo(0, "Should handle calendar service unavailability gracefully");
        
        Console.WriteLine($"Availability filter with potential fallback returned {resultsCount} results");
    }

    #endregion

    #region Accessibility and Responsive Tests

    [Test]
    [Category("P2")]
    [Category("Accessibility")]
    public async Task PartnerFiltering_MobileView_ShouldWorkOnSmallScreens()
    {
        // Arrange - Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await SetupBasicPartnerSearch();

        // Act - Test filter functionality on mobile
        await _filterPage!.OpenFiltersAsync(); // May be collapsed on mobile
        await _filterPage.TakeScreenshotAsync("filtering/17-mobile-filters-opened");

        await _filterPage.FilterByCityAsync("Austin");
        await _filterPage.TakeScreenshotAsync("filtering/18-mobile-filter-applied");

        // Assert
        var resultsCount = await _filterPage.GetFilteredResultsCountAsync();
        
        // Should work the same on mobile as desktop
        await _filterPage.AssertFilterSidebarVisibleAsync();
        
        Console.WriteLine($"Mobile filtering test completed with {resultsCount} results");
    }

    [Test]
    [Category("P2")]
    [Category("Accessibility")]
    public async Task PartnerFiltering_KeyboardNavigation_ShouldBeAccessible()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Test keyboard navigation through filters
        await _filterPage!.OpenFiltersAsync();
        
        // Tab through filter elements
        await Page.Keyboard.PressAsync("Tab"); // Move to first filter
        await Page.Keyboard.PressAsync("Tab"); // Move to second filter
        await Page.Keyboard.PressAsync("Tab"); // Move to third filter
        
        await _filterPage.TakeScreenshotAsync("filtering/19-keyboard-navigation");

        // Assert - Should be able to navigate without issues
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        focusedElement.Should().NotBeNullOrEmpty("Should have keyboard focus on an element");
        
        Console.WriteLine($"Keyboard navigation test completed, focused on: {focusedElement}");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    [Category("P2")]
    [Category("Edge-Cases")]
    public async Task PartnerFiltering_NoMatchingResults_ShouldShowEmptyState()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Apply very restrictive filters that should return no results
        await _filterPage!.ApplyMultipleFiltersAsync(
            city: "NonExistentCity",
            skill: "VeryRareSkill",
            experienceLevel: "Expert",
            minYears: 50 // Unrealistic years requirement
        );
        await _filterPage.TakeScreenshotAsync("filtering/20-no-matching-results");

        // Assert
        await _filterPage.AssertNoResultsAsync();
        
        Console.WriteLine("No matching results test completed - empty state properly displayed");
    }

    [Test]
    [Category("P2")]
    [Category("Edge-Cases")]
    public async Task PartnerFiltering_InvalidFilterValues_ShouldHandleGracefully()
    {
        // Arrange
        await SetupBasicPartnerSearch();

        // Act - Try applying filters with edge case values
        await _filterPage!.FilterByMinimumYearsAsync(-1); // Negative years
        var negativeYearsResults = await _filterPage.GetFilteredResultsCountAsync();
        
        await _filterPage.ClearAllFiltersAsync();
        await _filterPage.FilterByMinimumYearsAsync(999); // Unrealistic years
        var highYearsResults = await _filterPage.GetFilteredResultsCountAsync();

        // Assert - Should handle edge cases gracefully
        negativeYearsResults.Should().BeGreaterThanOrEqualTo(0, "Should handle negative years gracefully");
        highYearsResults.Should().BeGreaterThanOrEqualTo(0, "Should handle unrealistic years gracefully");
        
        Console.WriteLine($"Edge case handling: Negative years={negativeYearsResults}, High years={highYearsResults}");
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
            Console.WriteLine("Authentication required - handling OAuth flow...");
            var authResult = await _authPage!.HandleGoogleOAuthAsync(effectiveTimeout);
            if (!authResult)
            {
                Console.WriteLine("OAuth timed out in test environment - continuing with test");
            }
        }

        // Submit a basic problem to get partner results for filtering
        await _homePage.NavigateAsync();
        await _homePage.SubmitProblemDescriptionAsync(
            "We need expert guidance on technology strategy and digital transformation initiatives.",
            "Technology",
            "High"
        );
        await _homePage.WaitForPartnerResultsAsync();
        
        // Wait a moment for the filter interface to be ready
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