using Microsoft.Playwright;
using FluentAssertions;

namespace FxExpert.E2E.Tests.PageObjectModels;

public class AdvancedPartnerFilterPage : BasePage
{
    public AdvancedPartnerFilterPage(IPage page, string baseUrl = "https://localhost:8501") : base(page, baseUrl) { }

    #region Locators

    // Advanced filter container and controls
    private ILocator AdvancedFilterContainer => Page.Locator("[data-testid='advanced-partner-filter-container']")
        .Or(Page.Locator(".advanced-partner-filter-container"));
    private ILocator AdvancedFilterSidebar => Page.Locator("[data-testid='advanced-filter-sidebar']")
        .Or(Page.Locator(".filter-sidebar"));
    private ILocator MobileFilterButton => Page.Locator("[data-testid='mobile-filter-button']")
        .Or(Page.GetByRole(AriaRole.Button, new() { Name = "Open advanced filter options" }));
    private ILocator MobileDrawer => Page.Locator("[data-testid='mobile-filter-drawer']")
        .Or(Page.Locator(".mud-drawer"));
    private ILocator ClearAllFiltersButton => Page.Locator("[data-testid='clear-all-filters']")
        .Or(Page.GetByRole(AriaRole.Button, new() { Name = "Clear All Filters" }));

    // Expansion panels
    private ILocator IndustriesPanel => Page.Locator("text=🏢 Industries & Technologies").Or(Page.GetByText("Industries & Technologies"));
    private ILocator RateProjectPanel => Page.Locator("text=💰 Rate & Project Preferences").Or(Page.GetByText("Rate & Project Preferences"));
    private ILocator ExperienceTypesPanel => Page.Locator("text=⭐ Experience Types").Or(Page.GetByText("Experience Types"));
    private ILocator SpecializationsPanel => Page.Locator("text=🔧 Specializations").Or(Page.GetByText("Specializations"));

    // Industries & Technologies filters
    private ILocator IndustriesAutocomplete => Page.Locator("[data-testid='industries-filter']")
        .Or(Page.Locator("input[placeholder*='Industries']"));
    private ILocator TechnologiesAutocomplete => Page.Locator("[data-testid='technologies-filter']")
        .Or(Page.Locator("input[placeholder*='Technologies']"));
    private ILocator CertificationsAutocomplete => Page.Locator("[data-testid='certifications-filter']")
        .Or(Page.Locator("input[placeholder*='Certifications']"));

    // Rate & Project Preferences filters
    private ILocator MinRateSlider => Page.Locator("[data-testid='min-rate-slider']")
        .Or(Page.Locator("input[type='range']:near(text='Min Rate')"));
    private ILocator MaxRateSlider => Page.Locator("[data-testid='max-rate-slider']")
        .Or(Page.Locator("input[type='range']:near(text='Max Rate')"));
    private ILocator RateRangeDisplay => Page.Locator("[data-testid='rate-range-display']")
        .Or(Page.Locator(".rate-range-display"));
    private ILocator RemoteWorkCheckbox => Page.Locator("[data-testid='remote-work-checkbox']")
        .Or(Page.GetByLabel("Remote Work"));
    private ILocator OnSiteWorkCheckbox => Page.Locator("[data-testid='onsite-work-checkbox']")
        .Or(Page.GetByLabel("On-Site Work"));
    private ILocator TravelWillingnessCheckbox => Page.Locator("[data-testid='travel-willingness-checkbox']")
        .Or(Page.GetByLabel("Travel Willingness"));
    private ILocator LanguagesAutocomplete => Page.Locator("[data-testid='languages-filter']")
        .Or(Page.Locator("input[placeholder*='Languages']"));

    // Experience Types filters
    private ILocator SecurityClearanceCheckbox => Page.Locator("[data-testid='security-clearance-checkbox']")
        .Or(Page.GetByLabel("Has Security Clearance"));
    private ILocator ExecutiveExperienceCheckbox => Page.Locator("[data-testid='executive-experience-checkbox']")
        .Or(Page.GetByLabel("Executive Experience"));
    private ILocator StartupExperienceCheckbox => Page.Locator("[data-testid='startup-experience-checkbox']")
        .Or(Page.GetByLabel("Startup Experience"));
    private ILocator EnterpriseExperienceCheckbox => Page.Locator("[data-testid='enterprise-experience-checkbox']")
        .Or(Page.GetByLabel("Enterprise Experience"));
    private ILocator ConsultingExperienceCheckbox => Page.Locator("[data-testid='consulting-experience-checkbox']")
        .Or(Page.GetByLabel("Consulting Experience"));

    // Specializations filter
    private ILocator SpecializationsSelect => Page.Locator("[data-testid='specializations-filter']")
        .Or(Page.Locator("select[name*='specializations']"));

    // Filter statistics and results
    private ILocator FilterResultsCount => Page.Locator("[data-testid='filter-results-count']")
        .Or(Page.Locator(".filtered-count"));
    private ILocator ActiveFiltersCount => Page.Locator("[data-testid='active-filters-count']")
        .Or(Page.Locator(".active-filters-count"));
    private ILocator FilterResultsDisplay => Page.Locator("[data-testid='filter-results-display']")
        .Or(Page.Locator(".filter-results"));
    private ILocator ActiveFilterChips => Page.Locator("[data-testid='active-filter-chip']")
        .Or(Page.Locator(".mud-chip"));
    private ILocator MobileFilterBadge => Page.Locator("[data-testid='mobile-filter-badge']")
        .Or(Page.Locator(".mud-badge"));

    // Partner results
    private ILocator PartnerCards => Page.Locator("[data-testid='partner-card']").Or(Page.Locator(".partner-card"));
    private ILocator LoadingSpinner => Page.Locator("[data-testid='loading']").Or(Page.Locator(".mud-progress-circular"));
    private ILocator NoResultsMessage => Page.GetByText("No partners found").Or(Page.GetByText("No results"));

    #endregion

    #region Navigation and Setup

    public async Task OpenAdvancedFiltersAsync()
    {
        // On desktop, filters should be visible by default in sidebar
        // On mobile, need to open the drawer
        var viewportSize = Page.ViewportSize;
        if (viewportSize?.Width <= 960)
        {
            await OpenMobileFiltersAsync();
        }
        else
        {
            await AdvancedFilterSidebar.WaitForAsync();
        }
    }

    public async Task OpenMobileFiltersAsync()
    {
        if (await MobileFilterButton.IsVisibleAsync())
        {
            await MobileFilterButton.ClickAsync();
            await MobileDrawer.WaitForAsync();
        }
    }

    #endregion

    #region Industries & Technologies Filters

    public async Task FilterByIndustriesAsync(string[] industries)
    {
        await OpenAdvancedFiltersAsync();
        await IndustriesPanel.ClickAsync(); // Expand panel
        
        foreach (var industry in industries)
        {
            await IndustriesAutocomplete.FillAsync(industry);
            await Task.Delay(500); // Wait for autocomplete
            
            // Try to select from dropdown
            var industryOption = Page.GetByText(industry).First;
            if (await industryOption.IsVisibleAsync())
            {
                await industryOption.ClickAsync();
            }
            else
            {
                await IndustriesAutocomplete.PressAsync("Enter");
            }
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByTechnologiesAsync(string[] technologies)
    {
        await OpenAdvancedFiltersAsync();
        await IndustriesPanel.ClickAsync(); // Expand panel
        
        foreach (var technology in technologies)
        {
            await TechnologiesAutocomplete.FillAsync(technology);
            await Task.Delay(500); // Wait for autocomplete
            
            // Try to select from dropdown
            var techOption = Page.GetByText(technology).First;
            if (await techOption.IsVisibleAsync())
            {
                await techOption.ClickAsync();
            }
            else
            {
                await TechnologiesAutocomplete.PressAsync("Enter");
            }
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByCertificationsAsync(string[] certifications)
    {
        await OpenAdvancedFiltersAsync();
        await IndustriesPanel.ClickAsync(); // Expand panel
        
        foreach (var certification in certifications)
        {
            await CertificationsAutocomplete.FillAsync(certification);
            await Task.Delay(500); // Wait for autocomplete
            
            // Try to select from dropdown
            var certOption = Page.GetByText(certification).First;
            if (await certOption.IsVisibleAsync())
            {
                await certOption.ClickAsync();
            }
            else
            {
                await CertificationsAutocomplete.PressAsync("Enter");
            }
        }
        
        await WaitForFilterResultsAsync();
    }

    #endregion

    #region Rate & Project Preferences Filters

    public async Task FilterByRateRangeAsync(decimal minRate, decimal maxRate)
    {
        await OpenAdvancedFiltersAsync();
        await RateProjectPanel.ClickAsync(); // Expand panel
        
        // Set minimum rate using slider
        if (await MinRateSlider.IsVisibleAsync())
        {
            await MinRateSlider.FillAsync(minRate.ToString());
        }
        
        // Set maximum rate using slider
        if (await MaxRateSlider.IsVisibleAsync())
        {
            await MaxRateSlider.FillAsync(maxRate.ToString());
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByWorkPreferencesAsync(bool? remoteWork = null, bool? onSiteWork = null, bool? travelWillingness = null)
    {
        await OpenAdvancedFiltersAsync();
        await RateProjectPanel.ClickAsync(); // Expand panel
        
        if (remoteWork.HasValue)
        {
            await SetCheckboxAsync(RemoteWorkCheckbox, remoteWork.Value);
        }
        
        if (onSiteWork.HasValue)
        {
            await SetCheckboxAsync(OnSiteWorkCheckbox, onSiteWork.Value);
        }
        
        if (travelWillingness.HasValue)
        {
            await SetCheckboxAsync(TravelWillingnessCheckbox, travelWillingness.Value);
        }
        
        await WaitForFilterResultsAsync();
    }

    public async Task FilterByLanguagesAsync(string[] languages)
    {
        await OpenAdvancedFiltersAsync();
        await RateProjectPanel.ClickAsync(); // Expand panel
        
        foreach (var language in languages)
        {
            await LanguagesAutocomplete.FillAsync(language);
            await Task.Delay(500); // Wait for autocomplete
            
            // Try to select from dropdown
            var langOption = Page.GetByText(language).First;
            if (await langOption.IsVisibleAsync())
            {
                await langOption.ClickAsync();
            }
            else
            {
                await LanguagesAutocomplete.PressAsync("Enter");
            }
        }
        
        await WaitForFilterResultsAsync();
    }

    #endregion

    #region Experience Types Filters

    public async Task FilterByExperienceTypesAsync(
        bool? hasSecurityClearance = null,
        bool? executiveExperience = null,
        bool? startupExperience = null,
        bool? enterpriseExperience = null,
        bool? consultingExperience = null)
    {
        await OpenAdvancedFiltersAsync();
        await ExperienceTypesPanel.ClickAsync(); // Expand panel
        
        if (hasSecurityClearance.HasValue)
        {
            await SetCheckboxAsync(SecurityClearanceCheckbox, hasSecurityClearance.Value);
        }
        
        if (executiveExperience.HasValue)
        {
            await SetCheckboxAsync(ExecutiveExperienceCheckbox, executiveExperience.Value);
        }
        
        if (startupExperience.HasValue)
        {
            await SetCheckboxAsync(StartupExperienceCheckbox, startupExperience.Value);
        }
        
        if (enterpriseExperience.HasValue)
        {
            await SetCheckboxAsync(EnterpriseExperienceCheckbox, enterpriseExperience.Value);
        }
        
        if (consultingExperience.HasValue)
        {
            await SetCheckboxAsync(ConsultingExperienceCheckbox, consultingExperience.Value);
        }
        
        await WaitForFilterResultsAsync();
    }

    #endregion

    #region Specializations Filter

    public async Task FilterBySpecializationsAsync(string[] specializations)
    {
        await OpenAdvancedFiltersAsync();
        await SpecializationsPanel.ClickAsync(); // Expand panel
        
        foreach (var specialization in specializations)
        {
            // Find and select the specialization option
            var specializationOption = SpecializationsSelect.Locator($"option[value='{specialization}']");
            if (await specializationOption.IsVisibleAsync())
            {
                await SpecializationsSelect.SelectOptionAsync(specialization);
            }
        }
        
        await WaitForFilterResultsAsync();
    }

    #endregion

    #region Comprehensive Filter Application

    public async Task ApplyComprehensiveFiltersAsync(
        string[]? industries = null,
        string[]? technologies = null,
        string[]? certifications = null,
        decimal? minRate = null,
        decimal? maxRate = null,
        bool? remoteWork = null,
        bool? onSiteWork = null,
        bool? travelWillingness = null,
        string[]? languages = null,
        bool? hasSecurityClearance = null,
        bool? executiveExperience = null,
        bool? startupExperience = null,
        bool? enterpriseExperience = null,
        bool? consultingExperience = null,
        string[]? specializations = null)
    {
        if (industries != null)
            await FilterByIndustriesAsync(industries);
            
        if (technologies != null)
            await FilterByTechnologiesAsync(technologies);
            
        if (certifications != null)
            await FilterByCertificationsAsync(certifications);
            
        if (minRate.HasValue && maxRate.HasValue)
            await FilterByRateRangeAsync(minRate.Value, maxRate.Value);
            
        if (remoteWork.HasValue || onSiteWork.HasValue || travelWillingness.HasValue)
            await FilterByWorkPreferencesAsync(remoteWork, onSiteWork, travelWillingness);
            
        if (languages != null)
            await FilterByLanguagesAsync(languages);
            
        if (hasSecurityClearance.HasValue || executiveExperience.HasValue || 
            startupExperience.HasValue || enterpriseExperience.HasValue || 
            consultingExperience.HasValue)
        {
            await FilterByExperienceTypesAsync(
                hasSecurityClearance, executiveExperience, startupExperience, 
                enterpriseExperience, consultingExperience);
        }
            
        if (specializations != null)
            await FilterBySpecializationsAsync(specializations);
    }

    #endregion

    #region Filter Management

    public async Task ClearAllAdvancedFiltersAsync()
    {
        await OpenAdvancedFiltersAsync();
        if (await ClearAllFiltersButton.IsVisibleAsync())
        {
            await ClearAllFiltersButton.ClickAsync();
            await WaitForFilterResultsAsync();
        }
    }

    private async Task SetCheckboxAsync(ILocator checkbox, bool value)
    {
        var isChecked = await checkbox.IsCheckedAsync();
        if (isChecked != value)
        {
            await checkbox.ClickAsync();
        }
    }

    #endregion

    #region Results and Statistics

    public async Task WaitForFilterResultsAsync(int timeoutMs = 15000)
    {
        // Wait for loading to start and finish
        try
        {
            await LoadingSpinner.WaitForAsync(new() { Timeout = 2000 });
            await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // Loading might not appear if results are cached or very fast
        }
        
        // Wait for results to be visible or no results message
        try
        {
            await PartnerCards.First.WaitForAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // Might be no results - that's okay
        }
    }

    public async Task<int> GetFilteredResultsCountAsync()
    {
        try
        {
            await PartnerCards.First.WaitForAsync(new() { Timeout = 2000 });
            return await PartnerCards.CountAsync();
        }
        catch (TimeoutException)
        {
            return 0; // No results
        }
    }

    public async Task<int> GetActiveFiltersCountAsync()
    {
        if (await ActiveFiltersCount.IsVisibleAsync())
        {
            var countText = await ActiveFiltersCount.TextContentAsync();
            if (!string.IsNullOrEmpty(countText))
            {
                var match = System.Text.RegularExpressions.Regex.Match(countText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int count))
                {
                    return count;
                }
            }
        }
        return 0;
    }

    #endregion

    #region Assertions

    public async Task AssertActiveFiltersCountAsync(int expectedCount)
    {
        var actualCount = await GetActiveFiltersCountAsync();
        actualCount.Should().Be(expectedCount, $"Should have exactly {expectedCount} active filters");
    }

    public async Task AssertActiveFiltersCountGreaterThanAsync(int minCount)
    {
        var actualCount = await GetActiveFiltersCountAsync();
        actualCount.Should().BeGreaterThan(minCount, $"Should have more than {minCount} active filters");
    }

    public async Task AssertFilterResultsCountDisplayAsync(int expectedCount)
    {
        if (await FilterResultsCount.IsVisibleAsync())
        {
            var countText = await FilterResultsCount.TextContentAsync();
            countText.Should().Contain(expectedCount.ToString(), $"Should display {expectedCount} results");
        }
    }

    public async Task AssertRateRangeDisplayAsync(decimal minRate, decimal maxRate)
    {
        if (await RateRangeDisplay.IsVisibleAsync())
        {
            var rangeText = await RateRangeDisplay.TextContentAsync();
            rangeText.Should().Contain($"${minRate}", "Should display minimum rate");
            rangeText.Should().Contain($"${maxRate}", "Should display maximum rate");
        }
    }

    public async Task AssertMobileFilterBadgeVisibleAsync()
    {
        var viewportSize = Page.ViewportSize;
        if (viewportSize?.Width <= 960)
        {
            (await MobileFilterBadge.IsVisibleAsync()).Should().BeTrue("Mobile filter badge should be visible");
        }
    }

    public async Task AssertNoResultsAsync()
    {
        // Wait a bit to ensure no results are coming
        await Task.Delay(3000);
        
        var hasResults = await PartnerCards.First.IsVisibleAsync();
        var hasNoResultsMessage = await NoResultsMessage.IsVisibleAsync();
        
        (!hasResults || hasNoResultsMessage).Should().BeTrue("Should show no results when advanced filters don't match any partners");
    }

    #endregion
}