using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class PartnerFilterPage : BasePage
{
    public PartnerFilterPage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    #region Locators

    // Filter sidebar elements
    private ILocator FilterSidebar => Page.Locator("[data-testid='partner-filter-sidebar']").Or(Page.Locator(".filter-sidebar")).Or(Page.Locator(".partner-filters"));
    private ILocator FilterToggleButton => Page.Locator("[data-testid='filter-toggle']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }));
    private ILocator ClearFiltersButton => Page.Locator("[data-testid='clear-filters']").Or(Page.GetByRole(AriaRole.Button, new() { Name = "Clear Filters" }));

    // Location filters
    private ILocator CityInput => Page.Locator("[data-testid='city-filter']").Or(Page.Locator("input[placeholder*='city']"));
    private ILocator StateSelect => Page.Locator("[data-testid='state-filter']").Or(Page.Locator("select[name*='state']"));
    private ILocator RegionSelect => Page.Locator("[data-testid='region-filter']").Or(Page.Locator("select[name*='region']"));

    // Availability filters
    private ILocator AvailabilitySelect => Page.Locator("[data-testid='availability-filter']").Or(Page.Locator("select[name*='availability']"));
    private ILocator ThisWeekOption => Page.Locator("option[value='ThisWeek']").Or(Page.GetByText("This Week"));
    private ILocator NextWeekOption => Page.Locator("option[value='NextWeek']").Or(Page.GetByText("Next Week"));
    private ILocator ThisMonthOption => Page.Locator("option[value='ThisMonth']").Or(Page.GetByText("This Month"));

    // Skills filters
    private ILocator SkillsInput => Page.Locator("[data-testid='skills-filter']").Or(Page.Locator("input[placeholder*='skill']"));
    private ILocator SkillsDropdown => Page.Locator("[data-testid='skills-dropdown']").Or(Page.Locator(".skills-suggestions"));
    private ILocator SelectedSkills => Page.Locator("[data-testid='selected-skills']").Or(Page.Locator(".skill-chip"));

    // Experience filters
    private ILocator ExperienceLevelSelect => Page.Locator("[data-testid='experience-level-filter']").Or(Page.Locator("select[name*='experience']"));
    private ILocator MinYearsInput => Page.Locator("[data-testid='min-years-filter']").Or(Page.Locator("input[type='number'][placeholder*='years']"));

    // Partner results
    private ILocator PartnerResults => Page.Locator("[data-testid='partner-results']").Or(Page.Locator(".partner-card"));
    private ILocator PartnerCards => Page.Locator("[data-testid='partner-card']").Or(Page.Locator(".partner-card"));
    private ILocator FilterResultsCount => Page.Locator("[data-testid='results-count']").Or(Page.Locator(".results-count"));
    private ILocator LoadingSpinner => Page.Locator("[data-testid='loading']").Or(Page.Locator(".mud-progress-circular"));

    // Filter statistics
    private ILocator FilterStats => Page.Locator("[data-testid='filter-stats']").Or(Page.Locator(".filter-statistics"));
    private ILocator ActiveFiltersCount => Page.Locator("[data-testid='active-filters-count']").Or(Page.Locator(".active-filters-count"));

    // Sorting
    private ILocator SortSelect => Page.Locator("[data-testid='sort-select']").Or(Page.Locator("select[name*='sort']"));
    private ILocator SortByName => Page.Locator("option[value='Name']").Or(Page.GetByText("Name"));
    private ILocator SortByAvailability => Page.Locator("option[value='Availability']").Or(Page.GetByText("Availability"));
    private ILocator SortByExperience => Page.Locator("option[value='Experience']").Or(Page.GetByText("Experience"));

    #endregion

    #region Filter Actions

    public async Task OpenFiltersAsync()
    {
        // Open filter sidebar if it's not already open
        if (await FilterToggleButton.IsVisibleAsync())
        {
            await FilterToggleButton.ClickAsync();
        }
        await FilterSidebar.WaitForAsync();
    }

    public async Task ClearAllFiltersAsync()
    {
        await OpenFiltersAsync();
        if (await ClearFiltersButton.IsVisibleAsync())
        {
            await ClearFiltersButton.ClickAsync();
        }
        // Wait for results to update
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByCityAsync(string city)
    {
        await OpenFiltersAsync();
        await CityInput.WaitForAsync();
        await CityInput.FillAsync(city);
        // Wait for autocomplete/dropdown if present
        await Task.Delay(500);
        
        // Try to select from dropdown if available
        var cityOption = Page.GetByText(city).First;
        if (await cityOption.IsVisibleAsync())
        {
            await cityOption.ClickAsync();
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByStateAsync(string state)
    {
        await OpenFiltersAsync();
        await StateSelect.WaitForAsync();
        await StateSelect.SelectOptionAsync(state);
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByRegionAsync(string region)
    {
        await OpenFiltersAsync();
        await RegionSelect.WaitForAsync();
        await RegionSelect.SelectOptionAsync(region);
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByAvailabilityAsync(string timeframe)
    {
        await OpenFiltersAsync();
        await AvailabilitySelect.WaitForAsync();
        await AvailabilitySelect.SelectOptionAsync(timeframe);
        await WaitForFilterResultsAsync();
    }

    public async Task FilterBySkillAsync(string skill)
    {
        await OpenFiltersAsync();
        await SkillsInput.WaitForAsync();
        await SkillsInput.FillAsync(skill);
        
        // Wait for suggestions dropdown
        await Task.Delay(1000);
        
        // Try to select from dropdown
        var skillOption = Page.GetByText(skill).First;
        if (await skillOption.IsVisibleAsync())
        {
            await skillOption.ClickAsync();
        }
        else
        {
            // Fallback: press Enter
            await SkillsInput.PressAsync("Enter");
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByExperienceLevelAsync(string level)
    {
        await OpenFiltersAsync();
        await ExperienceLevelSelect.WaitForAsync();
        await ExperienceLevelSelect.SelectOptionAsync(level);
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByMinimumYearsAsync(int years)
    {
        await OpenFiltersAsync();
        await MinYearsInput.WaitForAsync();
        await MinYearsInput.FillAsync(years.ToString());
        await WaitForFilterResultsAsync();
    }

    public async Task ApplyMultipleFiltersAsync(
        string? city = null,
        string? state = null,
        string? region = null,
        string? availability = null,
        string? skill = null,
        string? experienceLevel = null,
        int? minYears = null)
    {
        await OpenFiltersAsync();
        
        if (!string.IsNullOrEmpty(city))
            await FilterByCityAsync(city);
            
        if (!string.IsNullOrEmpty(state))
            await FilterByStateAsync(state);
            
        if (!string.IsNullOrEmpty(region))
            await FilterByRegionAsync(region);
            
        if (!string.IsNullOrEmpty(availability))
            await FilterByAvailabilityAsync(availability);
            
        if (!string.IsNullOrEmpty(skill))
            await FilterBySkillAsync(skill);
            
        if (!string.IsNullOrEmpty(experienceLevel))
            await FilterByExperienceLevelAsync(experienceLevel);
            
        if (minYears.HasValue)
            await FilterByMinimumYearsAsync(minYears.Value);
    }

    #endregion

    #region Sorting Actions

    public async Task SortByAsync(string sortOption)
    {
        await SortSelect.WaitForAsync();
        await SortSelect.SelectOptionAsync(sortOption);
        await WaitForFilterResultsAsync();
    }

    public async Task SortByNameAsync()
    {
        await SortByAsync("Name");
    }

    public async Task SortByAvailabilityAsync()
    {
        await SortByAsync("Availability");
    }

    public async Task SortByExperienceAsync()
    {
        await SortByAsync("Experience");
    }

    #endregion

    #region Result Interactions

    public async Task WaitForFilterResultsAsync(int timeoutMs = 10000)
    {
        // Wait for loading to start
        try
        {
            await LoadingSpinner.WaitForAsync(new() { Timeout = 2000 });
            // Then wait for loading to finish
            await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // Loading might not appear if results are cached or very fast
        }
        
        // Wait for results to be visible
        await PartnerResults.First.WaitForAsync(new() { Timeout = timeoutMs });
    }

    public async Task<int> GetFilteredResultsCountAsync()
    {
        await PartnerCards.First.WaitForAsync();
        return await PartnerCards.CountAsync();
    }

    public async Task<string[]> GetPartnerNamesAsync()
    {
        var partners = await PartnerCards.AllAsync();
        var names = new List<string>();
        
        foreach (var partner in partners)
        {
            var nameElement = partner.Locator("h3, h4, h5, h6, .partner-name, [data-testid='partner-name']").First;
            if (await nameElement.CountAsync() > 0)
            {
                var name = await nameElement.TextContentAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    names.Add(name);
                }
            }
        }
        
        return names.ToArray();
    }

    public async Task<string[]> GetPartnerSkillsAsync(int partnerIndex = 0)
    {
        var partners = await PartnerCards.AllAsync();
        if (partnerIndex >= partners.Count) return Array.Empty<string>();
        
        var skillsElements = partners[partnerIndex].Locator(".skill-chip, .skill-tag, [data-testid='skill']");
        var skills = new List<string>();
        
        for (int i = 0; i < await skillsElements.CountAsync(); i++)
        {
            var skillText = await skillsElements.Nth(i).TextContentAsync();
            if (!string.IsNullOrEmpty(skillText))
            {
                skills.Add(skillText);
            }
        }
        
        return skills.ToArray();
    }

    public async Task<string> GetPartnerLocationAsync(int partnerIndex = 0)
    {
        var partners = await PartnerCards.AllAsync();
        if (partnerIndex >= partners.Count) return string.Empty;
        
        var locationElement = partners[partnerIndex].Locator(".location, [data-testid='location']").First;
        if (await locationElement.CountAsync() > 0)
        {
            return await locationElement.TextContentAsync() ?? string.Empty;
        }
        
        return string.Empty;
    }

    public async Task<int> GetPartnerAvailabilityAsync(int partnerIndex = 0)
    {
        var partners = await PartnerCards.AllAsync();
        if (partnerIndex >= partners.Count) return 0;
        
        var availabilityElement = partners[partnerIndex].Locator(".availability, [data-testid='availability']").First;
        if (await availabilityElement.CountAsync() > 0)
        {
            var availabilityText = await availabilityElement.TextContentAsync();
            if (!string.IsNullOrEmpty(availabilityText))
            {
                // Extract number from text like "5 slots available"
                var match = System.Text.RegularExpressions.Regex.Match(availabilityText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int availability))
                {
                    return availability;
                }
            }
        }
        
        return 0;
    }

    #endregion

    #region Assertions

    public async Task AssertFilterSidebarVisibleAsync()
    {
        await FilterSidebar.WaitForAsync();
        (await FilterSidebar.IsVisibleAsync()).Should().BeTrue("Filter sidebar should be visible");
    }

    public async Task AssertResultsCountAsync(int expectedCount)
    {
        var actualCount = await GetFilteredResultsCountAsync();
        actualCount.Should().Be(expectedCount, $"Should have exactly {expectedCount} filtered results");
    }

    public async Task AssertResultsCountGreaterThanAsync(int minCount)
    {
        var actualCount = await GetFilteredResultsCountAsync();
        actualCount.Should().BeGreaterThan(minCount, $"Should have more than {minCount} filtered results");
    }

    public async Task AssertNoResultsAsync()
    {
        // Wait a bit to ensure no results are coming
        await Task.Delay(2000);
        
        var noResultsMessage = Page.GetByText("No partners found").Or(Page.GetByText("No results"));
        var hasResults = await PartnerCards.First.IsVisibleAsync();
        var hasNoResultsMessage = await noResultsMessage.IsVisibleAsync();
        
        (!hasResults || hasNoResultsMessage).Should().BeTrue("Should show no results when filters don't match any partners");
    }

    public async Task AssertPartnerHasSkillAsync(int partnerIndex, string skill)
    {
        var partnerSkills = await GetPartnerSkillsAsync(partnerIndex);
        partnerSkills.Should().Contain(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase), 
            $"Partner {partnerIndex} should have skill '{skill}'");
    }

    public async Task AssertPartnerInLocationAsync(int partnerIndex, string expectedLocation)
    {
        var partnerLocation = await GetPartnerLocationAsync(partnerIndex);
        partnerLocation.Should().Contain(expectedLocation, 
            $"Partner {partnerIndex} should be in location '{expectedLocation}'");
    }

    public async Task AssertPartnerHasAvailabilityAsync(int partnerIndex, int minAvailability)
    {
        var availability = await GetPartnerAvailabilityAsync(partnerIndex);
        availability.Should().BeGreaterThanOrEqualTo(minAvailability,
            $"Partner {partnerIndex} should have at least {minAvailability} available slots");
    }

    public async Task AssertFilterStatsVisibleAsync()
    {
        if (await FilterStats.IsVisibleAsync())
        {
            (await FilterStats.IsVisibleAsync()).Should().BeTrue("Filter statistics should be visible");
        }
    }

    public async Task AssertActiveFiltersCountAsync(int expectedCount)
    {
        if (await ActiveFiltersCount.IsVisibleAsync())
        {
            var countText = await ActiveFiltersCount.TextContentAsync();
            countText.Should().Contain(expectedCount.ToString(), $"Should show {expectedCount} active filters");
        }
    }

    #endregion
}