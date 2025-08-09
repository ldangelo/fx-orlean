# Advanced Partner Search - Detailed Story Specifications

> **Epic**: EPIC-APS-001 - Advanced Partner Search  
> **Created**: 2025-08-09  
> **Last Updated**: 2025-08-09

## Story 1: Location-Based Filtering (APS-001)

### Story Details
- **Story ID**: APS-001
- **Epic**: EPIC-APS-001
- **Priority**: High
- **Effort**: 5 story points
- **Sprint**: Phase 1

### User Story
**As a** client looking for expert consultation  
**I want to** filter partners by city, state, or region  
**So that** I can find experts who are geographically convenient or understand my local market

### Acceptance Criteria
- [ ] **AC1**: Location filter panel appears after AI search results are displayed
- [ ] **AC2**: City filter with autocomplete dropdown populated from actual partner data
- [ ] **AC3**: State filter with dropdown selection of all available states
- [ ] **AC4**: Region filter with predefined regions ("West Coast", "East Coast", "Midwest", "South", "Other")
- [ ] **AC5**: Multiple location selections supported with OR logic (e.g., "California OR Washington")
- [ ] **AC6**: Real-time filtering updates results without new API calls
- [ ] **AC7**: Location filter state persists during user session
- [ ] **AC8**: Clear visual indication when location filters are active (filter count badge)
- [ ] **AC9**: Location badges visible on filtered partner cards
- [ ] **AC10**: "Clear Location Filters" functionality works correctly

### Technical Implementation

#### Frontend Changes
```razor
<!-- PartnerFilterComponent.razor -->
<MudExpansionPanel Text="📍 Location" IsInitiallyExpanded="true">
    <MudGrid>
        <MudItem xs="12" md="6">
            <MudAutocomplete T="string" 
                           Label="City" 
                           SearchFunc="SearchCities"
                           Value="_selectedCity"
                           ValueChanged="OnCityChanged"
                           Variant="Variant.Outlined"
                           Clearable="true" />
        </MudItem>
        <MudItem xs="12" md="6">
            <MudSelect T="string" 
                     Label="State" 
                     @bind-Value="_selectedState"
                     Variant="Variant.Outlined"
                     Clearable="true">
                @foreach (var state in _availableStates)
                {
                    <MudSelectItem Value="@state.Code">@state.Name</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
        <MudItem xs="12">
            <MudText Typo="Typo.subtitle2" Class="mb-2">Region</MudText>
            <MudChipSet T="string" MultiSelection="true" 
                       @bind-SelectedValues="_selectedRegions"
                       SelectionChanged="OnRegionSelectionChanged">
                <MudChip Text="West Coast" Value="west-coast" />
                <MudChip Text="East Coast" Value="east-coast" />
                <MudChip Text="Midwest" Value="midwest" />
                <MudChip Text="South" Value="south" />
                <MudChip Text="Other" Value="other" />
            </MudChipSet>
        </MudItem>
    </MudGrid>
</MudExpansionPanel>
```

#### Backend Changes
```csharp
public class PartnerFilterCriteria
{
    public List<string>? Cities { get; set; }
    public List<string>? States { get; set; }
    public List<string>? Regions { get; set; }
    // ... other properties
}

public static class RegionHelper
{
    private static readonly Dictionary<string, List<string>> RegionStates = new()
    {
        ["west-coast"] = new() { "CA", "OR", "WA", "AZ", "NV" },
        ["east-coast"] = new() { "NY", "NJ", "CT", "MA", "ME", "NH", "VT", "RI", "DE", "MD" },
        ["midwest"] = new() { "IL", "IN", "IA", "KS", "MI", "MN", "MO", "NE", "ND", "OH", "SD", "WI" },
        ["south"] = new() { "AL", "AR", "FL", "GA", "KY", "LA", "MS", "NC", "SC", "TN", "TX", "VA", "WV" }
    };
    
    public static List<string> GetStatesInRegion(string region) => 
        RegionStates.TryGetValue(region, out var states) ? states : new List<string>();
}
```

### Testing Requirements
```csharp
[Test]
public async Task LocationFilter_FiltersByCity_ReturnsCorrectPartners()
{
    // Arrange
    var partners = CreateTestPartnersWithLocations();
    var filter = new PartnerFilterCriteria { Cities = new() { "Seattle" } };
    
    // Act
    var filtered = await _filterService.ApplyFiltersAsync(partners, filter);
    
    // Assert
    Assert.That(filtered.All(p => p.City == "Seattle"));
}

[Test]  
public async Task LocationFilter_FiltersByRegion_ReturnsCorrectPartners()
{
    // Test region filtering logic
}
```

### Definition of Done
- [ ] All acceptance criteria implemented and tested
- [ ] Unit tests written and passing (>90% coverage)
- [ ] Manual testing completed on desktop and mobile
- [ ] Code review completed and approved
- [ ] Accessibility testing passed
- [ ] Performance testing confirms <100ms filter application

---

## Story 2: Availability-Based Filtering (APS-002)

### Story Details
- **Story ID**: APS-002
- **Epic**: EPIC-APS-001
- **Priority**: High
- **Effort**: 8 story points
- **Sprint**: Phase 2

### User Story
**As a** client with urgent needs  
**I want to** filter partners by their availability timeframe  
**So that** I can find experts who can accommodate my scheduling requirements

### Acceptance Criteria
- [ ] **AC1**: Availability filter with radio options: "Available This Week", "Available Next Week", "Available This Month"
- [ ] **AC2**: Integration with Google Calendar API to check real partner availability
- [ ] **AC3**: Real-time availability status indicators (Available/Limited/Busy) with color coding
- [ ] **AC4**: Visual availability badges on partner cards (🟢 Available, 🟡 Limited, 🔴 Busy)
- [ ] **AC5**: "Sort by Earliest Available" option in results sorting
- [ ] **AC6**: Availability filter combines seamlessly with location and skills filters
- [ ] **AC7**: Display next available time slot for each partner
- [ ] **AC8**: Proper timezone handling for availability calculations
- [ ] **AC9**: Graceful degradation when calendar API is unavailable
- [ ] **AC10**: Caching of availability data for performance (5-minute cache)

### Technical Implementation

#### Frontend Changes
```razor
<MudExpansionPanel Text="📅 Availability">
    <MudRadioGroup T="AvailabilityTimeframe?" @bind-Value="_selectedAvailability">
        <MudRadio Option="@AvailabilityTimeframe.ThisWeek" Color="Color.Success">
            This Week (Available in 1-7 days)
        </MudRadio>
        <MudRadio Option="@AvailabilityTimeframe.NextWeek" Color="Color.Info">
            Next Week (Available in 8-14 days)
        </MudRadio>
        <MudRadio Option="@AvailabilityTimeframe.ThisMonth" Color="Color.Warning">
            This Month (Available in 1-30 days)
        </MudRadio>
        <MudRadio Option="@((AvailabilityTimeframe?)null)">
            Any Availability
        </MudRadio>
    </MudRadioGroup>
    
    <MudDivider Class="my-3" />
    
    <MudText Typo="Typo.caption" Class="mb-2">
        Availability is checked in real-time based on partner calendars
    </MudText>
</MudExpansionPanel>
```

#### Enhanced Partner Cards
```razor
<MudCardHeader>
    <CardHeaderContent>
        <MudText Typo="Typo.h6">@partner.GetFullName()</MudText>
        <MudStack Direction="Row" Spacing="2" Class="mt-1">
            <MudChip Size="Size.Small" 
                   Color="@GetAvailabilityColor(partner.CurrentAvailability)"
                   Icon="@GetAvailabilityIcon(partner.CurrentAvailability)">
                @GetAvailabilityText(partner.CurrentAvailability)
            </MudChip>
            @if (partner.NextAvailableSlot != default)
            {
                <MudChip Size="Size.Small" Color="Color.Default" Variant="Variant.Outlined">
                    Next: @partner.NextAvailableSlot.ToString("MMM d, h:mm tt")
                </MudChip>
            }
        </MudStack>
    </CardHeaderContent>
</MudCardHeader>
```

#### Backend Service Enhancement
```csharp
public interface IAvailabilityService
{
    Task<PartnerAvailability> GetPartnerAvailabilityAsync(string partnerEmail);
    Task<Dictionary<string, PartnerAvailability>> GetBulkAvailabilityAsync(List<string> partnerEmails);
    Task<List<string>> GetAvailablePartnersAsync(AvailabilityTimeframe timeframe);
}

public class AvailabilityService : IAvailabilityService
{
    private readonly GoogleCalendarService _calendarService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AvailabilityService> _logger;
    
    public async Task<PartnerAvailability> GetPartnerAvailabilityAsync(string partnerEmail)
    {
        var cacheKey = $"availability_{partnerEmail}";
        
        if (_cache.TryGetValue(cacheKey, out PartnerAvailability? cached))
            return cached!;
            
        try
        {
            // Get partner's calendar events for next 30 days
            var events = await _calendarService.GetPartnerEventsAsync(partnerEmail, 
                DateTime.Today, DateTime.Today.AddDays(30));
                
            var availability = CalculateAvailability(events);
            
            // Cache for 5 minutes
            _cache.Set(cacheKey, availability, TimeSpan.FromMinutes(5));
            
            return availability;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get availability for partner {Email}", partnerEmail);
            
            // Return default availability on error
            return new PartnerAvailability
            {
                PartnerEmail = partnerEmail,
                Status = AvailabilityStatus.Limited,
                NextAvailableSlot = DateTime.Today.AddDays(7).AddHours(10)
            };
        }
    }
    
    private PartnerAvailability CalculateAvailability(List<CalendarEvent> events)
    {
        // Business logic to calculate availability based on calendar events
        // Consider working hours, existing bookings, time zone, etc.
    }
}

public enum AvailabilityTimeframe
{
    ThisWeek,
    NextWeek, 
    ThisMonth
}

public class PartnerAvailability
{
    public string PartnerEmail { get; set; } = string.Empty;
    public AvailabilityStatus Status { get; set; }
    public DateTime NextAvailableSlot { get; set; }
    public int AvailableSlotsThisWeek { get; set; }
    public int AvailableSlotsNextWeek { get; set; }
    public int AvailableSlotsThisMonth { get; set; }
    public List<TimeSlot> UpcomingSlots { get; set; } = new();
}

public enum AvailabilityStatus
{
    Available,    // 5+ slots available this week
    Limited,      // 1-4 slots available this week  
    Busy         // 0 slots available this week
}
```

### Testing Requirements
```csharp
[Test]
public async Task AvailabilityFilter_ThisWeek_ReturnsAvailablePartners()
{
    // Test filtering by this week availability
}

[Test]
public async Task AvailabilityService_WithCalendarEvents_CalculatesCorrectAvailability()
{
    // Test availability calculation logic
}

[Test]
public async Task AvailabilityService_WithCachedData_ReturnsCachedResults()
{
    // Test caching mechanism
}

[Test]  
public async Task AvailabilityService_WithAPIFailure_ReturnsGracefulFallback()
{
    // Test error handling and fallback
}
```

### Definition of Done
- [ ] All acceptance criteria implemented and tested
- [ ] Calendar API integration working correctly
- [ ] Caching mechanism implemented and tested
- [ ] Error handling and graceful degradation tested
- [ ] Performance testing shows <300ms response time
- [ ] Mobile responsive design verified

---

## Story 3: Skills-Based Filtering (APS-003)

### Story Details
- **Story ID**: APS-003
- **Epic**: EPIC-APS-001
- **Priority**: High
- **Effort**: 5 story points
- **Sprint**: Phase 1

### User Story
**As a** client with specific expertise needs  
**I want to** filter partners by their skills and experience levels  
**So that** I can find experts with the exact capabilities I need

### Acceptance Criteria
- [ ] **AC1**: Skills filter with searchable/autocomplete skill input
- [ ] **AC2**: Multi-select skills filtering with AND logic (partner must have ALL selected skills)
- [ ] **AC3**: Experience level filter dropdown (Novice, Beginner, Proficient, Expert)
- [ ] **AC4**: Years of experience range slider (0-30 years)
- [ ] **AC5**: Popular skills as quick-select chips/buttons
- [ ] **AC6**: Skills organized by categories (Technical, Leadership, Industry)
- [ ] **AC7**: Display skill match percentage on filtered partner cards
- [ ] **AC8**: Highlight matching skills on partner cards
- [ ] **AC9**: "Clear Skills Filters" functionality
- [ ] **AC10**: Skills filter combines with location and availability filters

### Technical Implementation

#### Frontend Component
```razor
<MudExpansionPanel Text="🎯 Skills & Experience">
    <MudStack Spacing="3">
        <!-- Skills Search -->
        <MudAutocomplete T="string"
                       Label="Search Skills"
                       SearchFunc="SearchSkills"
                       MultiSelection="true"
                       @bind-SelectedValues="_selectedSkills"
                       SelectionChanged="OnSkillsChanged"
                       Variant="Variant.Outlined"
                       Clearable="true"
                       HelperText="Select specific skills (AND logic - partner must have all selected skills)" />
        
        <!-- Popular Skills Quick Select -->
        <div>
            <MudText Typo="Typo.subtitle2" Class="mb-2">Popular Skills</MudText>
            <MudChipSet MultiSelection="true" @bind-SelectedValues="_quickSelectSkills">
                <MudChip Text="Cloud Architecture" Value="Cloud Architecture" />
                <MudChip Text="Cybersecurity" Value="Cybersecurity" />
                <MudChip Text="Digital Transformation" Value="Digital Transformation" />
                <MudChip Text="Data Strategy" Value="Data Strategy" />
                <MudChip Text="IT Governance" Value="IT Governance" />
                <MudChip Text="Enterprise Architecture" Value="Enterprise Architecture" />
            </MudChipSet>
        </div>
        
        <!-- Experience Level -->
        <MudSelect T="ExperienceLevel?" 
                 @bind-Value="_minExperienceLevel"
                 Label="Minimum Experience Level"
                 Variant="Variant.Outlined"
                 Clearable="true">
            <MudSelectItem Value="ExperienceLevel.Novice">Novice</MudSelectItem>
            <MudSelectItem Value="ExperienceLevel.Beginner">Beginner</MudSelectItem>
            <MudSelectItem Value="ExperienceLevel.Proficient">Proficient</MudSelectItem>
            <MudSelectItem Value="ExperienceLevel.Expert">Expert</MudSelectItem>
        </MudSelect>
        
        <!-- Years of Experience -->
        <div>
            <MudText Typo="Typo.subtitle2" Class="mb-2">
                Minimum Years of Experience: @_minYearsExperience years
            </MudText>
            <MudSlider T="int" 
                     @bind-Value="_minYearsExperience"
                     Min="0" Max="30" Step="1"
                     ValueChanged="OnYearsExperienceChanged"
                     Color="Color.Primary" />
        </div>
        
        <!-- Skills Categories -->
        <MudExpansionPanels>
            <MudExpansionPanel Text="Technical Skills">
                <MudChipSet MultiSelection="true" @bind-SelectedValues="_technicalSkills">
                    <MudChip Text="Software Development" Value="Software Development" />
                    <MudChip Text="System Administration" Value="System Administration" />
                    <MudChip Text="Database Management" Value="Database Management" />
                    <!-- More technical skills -->
                </MudChipSet>
            </MudExpansionPanel>
            
            <MudExpansionPanel Text="Leadership Skills">
                <MudChipSet MultiSelection="true" @bind-SelectedValues="_leadershipSkills">
                    <MudChip Text="Strategic Planning" Value="Strategic Planning" />
                    <MudChip Text="Team Management" Value="Team Management" />
                    <MudChip Text="Change Management" Value="Change Management" />
                    <!-- More leadership skills -->
                </MudChipSet>
            </MudExpansionPanel>
            
            <MudExpansionPanel Text="Industry Skills">
                <MudChipSet MultiSelection="true" @bind-SelectedValues="_industrySkills">
                    <MudChip Text="Financial Services" Value="Financial Services" />
                    <MudChip Text="Healthcare IT" Value="Healthcare IT" />
                    <MudChip Text="Retail Technology" Value="Retail Technology" />
                    <!-- More industry skills -->
                </MudChipSet>
            </MudExpansionPanel>
        </MudExpansionPanels>
    </MudStack>
</MudExpansionPanel>
```

#### Enhanced Partner Cards with Skill Matching
```razor
<MudCardContent>
    <!-- Existing content -->
    
    <!-- Skills Section with Highlighting -->
    <MudStack Direction="Row" Spacing="1" Class="mb-2" Wrap="Wrap.Wrap">
        <MudText Typo="Typo.caption" Class="align-self-center"><b>Skills:</b></MudText>
        @foreach (var skill in GetDisplaySkills(partner))
        {
            <MudChip Size="Size.Small" 
                   Color="@GetSkillChipColor(skill, _appliedFilters?.RequiredSkills)"
                   Variant="@GetSkillChipVariant(skill, _appliedFilters?.RequiredSkills)">
                @skill.Skill
                @if (skill.YearsOfExperience > 0)
                {
                    <text> (@skill.YearsOfExperience yr)</text>
                }
            </MudChip>
        }
    </MudStack>
    
    <!-- Skill Match Percentage -->
    @if (_appliedFilters?.RequiredSkills?.Any() == true)
    {
        var matchPercentage = CalculateSkillMatchPercentage(partner, _appliedFilters.RequiredSkills);
        <MudProgressLinear Value="matchPercentage" Color="Color.Success" Size="Size.Small" Class="mb-2">
            <MudText Typo="Typo.caption">@matchPercentage% Skill Match</MudText>
        </MudProgressLinear>
    }
</MudCardContent>
```

#### Backend Filtering Logic
```csharp
public class FilterService : IFilterService
{
    public async Task<List<Partner>> ApplySkillsFilterAsync(List<Partner> partners, SkillFilterCriteria criteria)
    {
        var filtered = partners.AsQueryable();
        
        // Required skills filter (AND logic)
        if (criteria.RequiredSkills?.Any() == true)
        {
            filtered = filtered.Where(partner =>
                criteria.RequiredSkills.All(requiredSkill =>
                    partner.Skills.Any(partnerSkill =>
                        partnerSkill.Skill.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase))));
        }
        
        // Minimum experience level filter
        if (criteria.MinExperienceLevel.HasValue)
        {
            filtered = filtered.Where(partner =>
                partner.Skills.Any(skill => skill.ExperienceLevel >= criteria.MinExperienceLevel));
        }
        
        // Minimum years of experience filter
        if (criteria.MinYearsExperience.HasValue)
        {
            filtered = filtered.Where(partner =>
                partner.Skills.Any(skill => skill.YearsOfExperience >= criteria.MinYearsExperience));
        }
        
        return filtered.ToList();
    }
    
    public int CalculateSkillMatchPercentage(Partner partner, List<string> requiredSkills)
    {
        if (!requiredSkills?.Any() == true)
            return 100;
            
        var matchingSkills = requiredSkills.Count(requiredSkill =>
            partner.Skills.Any(partnerSkill =>
                partnerSkill.Skill.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase)));
                
        return (int)Math.Round((double)matchingSkills / requiredSkills.Count * 100);
    }
    
    public async Task<List<string>> GetAvailableSkillsAsync(List<Partner> partners)
    {
        return partners
            .SelectMany(p => p.Skills.Select(s => s.Skill))
            .Distinct()
            .OrderBy(s => s)
            .ToList();
    }
}

public class SkillFilterCriteria
{
    public List<string>? RequiredSkills { get; set; }
    public ExperienceLevel? MinExperienceLevel { get; set; }
    public int? MinYearsExperience { get; set; }
    public List<string>? TechnicalSkills { get; set; }
    public List<string>? LeadershipSkills { get; set; }
    public List<string>? IndustrySkills { get; set; }
}
```

### Testing Requirements
```csharp
[Test]
public async Task SkillsFilter_WithRequiredSkills_ReturnsOnlyMatchingPartners()
{
    // Test AND logic for required skills
}

[Test]
public async Task SkillsFilter_WithMinExperienceLevel_FiltersCorrectly()
{
    // Test experience level filtering
}

[Test]
public void CalculateSkillMatchPercentage_WithPartialMatch_ReturnsCorrectPercentage()
{
    // Test skill match percentage calculation
}

[Test]
public async Task GetAvailableSkills_WithPartnerData_ReturnsDistinctSkills()
{
    // Test skill aggregation for autocomplete
}
```

### Definition of Done
- [ ] All acceptance criteria implemented and tested
- [ ] Skills search and autocomplete working
- [ ] Multi-select filtering with AND logic
- [ ] Experience level and years filtering
- [ ] Skill match percentage calculation
- [ ] Popular skills quick-select
- [ ] Skills categorization
- [ ] Unit tests with >90% coverage

---

## Story 4: Multi-Faceted Search Interface (APS-004)

### Story Details
- **Story ID**: APS-004
- **Epic**: EPIC-APS-001
- **Priority**: High
- **Effort**: 8 story points
- **Sprint**: Phase 3

### User Story
**As a** client seeking the perfect partner match  
**I want to** apply multiple filters simultaneously with a responsive interface  
**So that** I can narrow down results using all my criteria at once

### Acceptance Criteria
- [ ] **AC1**: Responsive filter sidebar that works on desktop (300px width) and mobile (bottom drawer)
- [ ] **AC2**: Filter combination uses appropriate logic (AND/OR where suitable)
- [ ] **AC3**: Active filter indicator with live count of matching results
- [ ] **AC4**: "Clear All Filters" button with confirmation dialog
- [ ] **AC5**: Filter state persistence during browser session (sessionStorage)
- [ ] **AC6**: URL state management for shareable filtered results
- [ ] **AC7**: Smooth filter animations and transitions (300ms duration)
- [ ] **AC8**: WCAG 2.1 AA accessibility compliance
- [ ] **AC9**: Keyboard navigation support for all filter controls
- [ ] **AC10**: Loading states during filter application with skeleton UI

### Technical Implementation

#### Main Filter Component Structure
```razor
@* PartnerFilterComponent.razor *@
<div class="partner-filter-container">
    @if (_isDesktop)
    {
        <!-- Desktop Sidebar -->
        <MudPaper Class="filter-sidebar pa-4" Style="width: 300px; height: fit-content;">
            @FilterContent
        </MudPaper>
    }
    else
    {
        <!-- Mobile Bottom Drawer -->
        <MudDrawer @bind-Open="_mobileDrawerOpen" 
                   Anchor="Anchor.Bottom" 
                   Height="70vh" 
                   ClipMode="DrawerClipMode.Always">
            <div class="pa-4">
                @FilterContent
            </div>
        </MudDrawer>
        
        <!-- Mobile Filter Button -->
        <MudFab Color="Color.Primary" 
               Icon="@Icons.Material.Filled.FilterList"
               OnClick="@(() => _mobileDrawerOpen = true)"
               Style="position: fixed; bottom: 20px; right: 20px; z-index: 1000;">
            @if (_activeFilterCount > 0)
            {
                <MudBadge Content="@_activeFilterCount" Color="Color.Error" 
                         Style="position: absolute; top: -8px; right: -8px;" />
            }
        </MudFab>
    }
</div>

@code {
    private bool _isDesktop = true;
    private bool _mobileDrawerOpen = false;
    private int _activeFilterCount = 0;
    
    private RenderFragment FilterContent => @<div>
        <!-- Filter Header -->
        <MudStack Direction="Row" Justify="Justify.SpaceBetween" AlignItems="Center" Class="mb-4">
            <MudText Typo="Typo.h6">Filter Results</MudText>
            @if (_activeFilterCount > 0)
            {
                <MudChip Color="Color.Primary" Size="Size.Small">
                    @_filteredPartners.Count results
                </MudChip>
            }
        </MudStack>
        
        <!-- Active Filters Summary -->
        @if (_activeFilterCount > 0)
        {
            <MudPaper Class="pa-3 mb-3" Style="background-color: var(--mud-palette-info-lighten);">
                <MudStack Direction="Row" Spacing="1" Wrap="Wrap.Wrap">
                    <MudText Typo="Typo.caption" Class="align-self-center">
                        <strong>Active Filters:</strong>
                    </MudText>
                    @RenderActiveFilterChips()
                </MudStack>
                
                <MudButton Size="Size.Small" 
                         Color="Color.Error" 
                         Variant="Variant.Text"
                         OnClick="ClearAllFilters"
                         StartIcon="@Icons.Material.Filled.Clear"
                         Class="mt-2">
                    Clear All Filters
                </MudButton>
            </MudPaper>
        }
        
        <!-- Filter Panels -->
        <MudExpansionPanels>
            @LocationFilterPanel
            @AvailabilityFilterPanel  
            @SkillsFilterPanel
        </MudExpansionPanels>
        
        <!-- Apply/Reset Buttons -->
        <MudStack Direction="Row" Justify="Justify.SpaceBetween" Class="mt-4">
            <MudButton Variant="Variant.Outlined" 
                     OnClick="ResetFilters"
                     Disabled="@(_activeFilterCount == 0)">
                Reset
            </MudButton>
            <MudButton Variant="Variant.Filled" 
                     Color="Color.Primary" 
                     OnClick="ApplyFilters">
                Apply Filters
            </MudButton>
        </MudStack>
    </div>;
}
```

#### URL State Management
```csharp
public class FilterStateManager
{
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    
    public async Task SaveFilterStateAsync(PartnerFilterCriteria criteria)
    {
        // Save to sessionStorage
        var json = JsonSerializer.Serialize(criteria);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "partnerFilters", json);
        
        // Update URL with filter parameters
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
        var query = BuildQueryString(criteria);
        var newUrl = $"{uri.GetLeftPart(UriPartial.Path)}?{query}";
        
        _navigationManager.NavigateTo(newUrl, replace: true);
    }
    
    public async Task<PartnerFilterCriteria?> LoadFilterStateAsync()
    {
        // Try to load from URL parameters first
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        
        if (queryParams.Count > 0)
        {
            return ParseQueryString(queryParams);
        }
        
        // Fall back to sessionStorage
        var json = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "partnerFilters");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonSerializer.Deserialize<PartnerFilterCriteria>(json);
        }
        
        return null;
    }
    
    private string BuildQueryString(PartnerFilterCriteria criteria)
    {
        var queryParams = new List<string>();
        
        if (criteria.Cities?.Any() == true)
            queryParams.Add($"cities={string.Join(",", criteria.Cities)}");
            
        if (criteria.States?.Any() == true)
            queryParams.Add($"states={string.Join(",", criteria.States)}");
            
        if (criteria.RequiredSkills?.Any() == true)
            queryParams.Add($"skills={string.Join(",", criteria.RequiredSkills)}");
            
        if (criteria.Availability.HasValue)
            queryParams.Add($"availability={criteria.Availability}");
            
        return string.Join("&", queryParams);
    }
}
```

#### Responsive Design Implementation
```scss
.partner-filter-container {
    .filter-sidebar {
        @media (max-width: 960px) {
            display: none;
        }
    }
    
    .mobile-filter-fab {
        @media (min-width: 961px) {
            display: none;
        }
    }
}

// Filter animations
.filter-panel-enter {
    transform: translateX(-100%);
    opacity: 0;
}

.filter-panel-enter-active {
    transform: translateX(0);
    opacity: 1;
    transition: all 300ms ease-in-out;
}

.filter-results-transition {
    transition: all 200ms ease-in-out;
}
```

#### Accessibility Implementation
```razor
<!-- ARIA labels and keyboard support -->
<div role="region" aria-label="Partner search filters">
    <MudExpansionPanels>
        <MudExpansionPanel Text="Location" 
                          AriaLabel="Location filter options"
                          role="group">
            <MudAutocomplete T="string" 
                           Label="City"
                           aria-label="Filter by city"
                           aria-describedby="city-help-text" />
            <MudText Id="city-help-text" Typo="Typo.caption">
                Type to search and select cities
            </MudText>
        </MudExpansionPanel>
    </MudExpansionPanels>
    
    <MudButton OnClick="ApplyFilters"
               aria-label="Apply selected filters to search results"
               tabindex="0">
        Apply Filters
    </MudButton>
</div>

<!-- Skip link for keyboard users -->
<MudButton Class="sr-only" 
           Style="position: absolute; left: -9999px;"
           tabindex="1"
           OnFocus="@(() => { /* Focus on main content */ })">
    Skip to search results
</MudButton>
```

### Testing Requirements
```csharp
[Test]
public async Task FilterInterface_OnDesktop_ShowsSidebar()
{
    // Test desktop layout
}

[Test]
public async Task FilterInterface_OnMobile_ShowsBottomDrawer()
{
    // Test mobile responsive design
}

[Test]
public async Task FilterStateManager_SavesAndLoadsState_Correctly()
{
    // Test state persistence
}

[Test]
public async Task URLStateManagement_WithFilters_UpdatesURLCorrectly()
{
    // Test URL state management
}

[Test]
public async Task AccessibilityCompliance_AllControls_AreKeyboardNavigable()
{
    // Test keyboard navigation
}
```

### Definition of Done
- [ ] Responsive design working on all screen sizes
- [ ] Filter state persistence implemented
- [ ] URL sharing functionality working
- [ ] Accessibility compliance verified
- [ ] Smooth animations and transitions
- [ ] All browser testing completed
- [ ] Performance testing passed

---

## Story 5: Enhanced Results Display (APS-005)

### Story Details
- **Story ID**: APS-005
- **Epic**: EPIC-APS-001
- **Priority**: Medium
- **Effort**: 5 story points
- **Sprint**: Phase 3

### User Story
**As a** client reviewing filtered results  
**I want to** see enhanced partner cards with filter-relevant information and sorting options  
**So that** I can make informed decisions quickly

### Acceptance Criteria
- [ ] **AC1**: Partner cards prominently display location badges
- [ ] **AC2**: Availability status indicators with clear color coding
- [ ] **AC3**: Top 3 matching skills highlighted on each card
- [ ] **AC4**: Enhanced "Why this matches" explanation includes filter criteria
- [ ] **AC5**: Sort options: Relevance (default), Availability, Experience, Location proximity
- [ ] **AC6**: Pagination or infinite scroll for result sets >20 partners
- [ ] **AC7**: Loading states during sort/filter with skeleton cards
- [ ] **AC8**: Empty state messaging when no results match filters
- [ ] **AC9**: Results counter showing "X of Y partners match your criteria"
- [ ] **AC10**: "Save Search" functionality for logged-in users

### Technical Implementation

#### Enhanced Partner Card Component
```razor
@* Enhanced PartnerCard.razor *@
<MudCard Class="partner-card @(_isFiltered ? "filtered-card" : "")" 
         Style="@GetCardStyle()">
    <MudCardHeader>
        <CardHeaderAvatar>
            <MudAvatar Color="Color.Primary" Size="Size.Large">
                @partner.FirstName.Substring(0, 1)@partner.LastName.Substring(0, 1)
            </MudAvatar>
        </CardHeaderAvatar>
        <CardHeaderContent>
            <MudStack Direction="Row" Justify="Justify.SpaceBetween" AlignItems="Center">
                <div>
                    <MudText Typo="Typo.h6">@partner.GetFullName()</MudText>
                    <MudText Typo="Typo.subtitle1">@partner.Title</MudText>
                </div>
                
                <!-- Status Badges -->
                <MudStack Direction="Column" Spacing="1" AlignItems="Center">
                    <!-- Availability Badge -->
                    <MudChip Size="Size.Small" 
                           Color="@GetAvailabilityColor(partner.CurrentAvailability)"
                           Icon="@GetAvailabilityIcon(partner.CurrentAvailability)">
                        @GetAvailabilityText(partner.CurrentAvailability)
                    </MudChip>
                    
                    <!-- Location Badge -->
                    <MudChip Size="Size.Small" 
                           Color="Color.Info" 
                           Icon="@Icons.Material.Filled.LocationOn"
                           Variant="Variant.Outlined">
                        @partner.GetLocation()
                    </MudChip>
                </MudStack>
            </MudStack>
        </CardHeaderContent>
    </MudCardHeader>
    
    <MudCardContent>
        <!-- Enhanced Why This Matches Section -->
        @if (!string.IsNullOrEmpty(_enhancedMatchingReason))
        {
            <MudPaper Class="pa-3 mb-3" 
                     Style="background-color: var(--mud-palette-success-lighten); border-left: 4px solid var(--mud-palette-success);">
                <MudStack Direction="Row" Spacing="2">
                    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" />
                    <div>
                        <MudText Typo="Typo.subtitle2"><strong>Why this expert matches your criteria:</strong></MudText>
                        <MudText Typo="Typo.body2">@_enhancedMatchingReason</MudText>
                    </div>
                </MudStack>
            </MudPaper>
        }
        
        <!-- Skills Section with Match Highlighting -->
        <div class="mb-3">
            <MudText Typo="Typo.subtitle2" Class="mb-2">
                <MudIcon Icon="@Icons.Material.Filled.Star" Size="Size.Small" Class="mr-1" />
                Top Skills
                @if (_skillMatchPercentage > 0)
                {
                    <MudChip Size="Size.Small" Color="Color.Success" Class="ml-2">
                        @_skillMatchPercentage% Match
                    </MudChip>
                }
            </MudText>
            
            <MudStack Direction="Row" Spacing="1" Wrap="Wrap.Wrap">
                @foreach (var skill in GetTopMatchingSkills(partner, 3))
                {
                    <MudChip Size="Size.Small" 
                           Color="@GetSkillChipColor(skill)"
                           Variant="@GetSkillChipVariant(skill)"
                           Icon="@GetSkillIcon(skill)">
                        @skill.Skill (@skill.YearsOfExperience yr)
                    </MudChip>
                }
            </MudStack>
        </div>
        
        <!-- Experience Summary -->
        <MudStack Direction="Row" Justify="Justify.SpaceBetween" AlignItems="Center" Class="mb-2">
            <MudText Typo="Typo.body2">
                <MudIcon Icon="@Icons.Material.Filled.Work" Size="Size.Small" Class="mr-1" />
                @GetExperienceSummary(partner)
            </MudText>
            
            @if (partner.NextAvailableSlot != default)
            {
                <MudText Typo="Typo.caption" Style="color: var(--mud-palette-success);">
                    <MudIcon Icon="@Icons.Material.Filled.Schedule" Size="Size.Small" Class="mr-1" />
                    Next: @partner.NextAvailableSlot.ToString("MMM d")
                </MudText>
            }
        </MudStack>
        
        <!-- Filter Match Indicators -->
        @if (_appliedFilters != null)
        {
            <MudDivider Class="my-2" />
            <MudStack Direction="Row" Spacing="1" Wrap="Wrap.Wrap">
                @RenderFilterMatchIndicators()
            </MudStack>
        }
    </MudCardContent>
    
    <MudCardActions>
        <MudButton Variant="Variant.Text" 
                 Color="Color.Primary"
                 OnClick="@(() => ViewProfile(partner))">
            View Profile
        </MudButton>
        <MudButton Variant="Variant.Filled" 
                 Color="Color.Primary"
                 EndIcon="@Icons.Material.Filled.CalendarToday"
                 OnClick="@(() => ScheduleConsultation(partner))">
            Schedule Consultation
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    private string _enhancedMatchingReason = string.Empty;
    private int _skillMatchPercentage;
    
    private string GetCardStyle()
    {
        if (_isFiltered && _skillMatchPercentage > 80)
            return "box-shadow: 0 4px 20px rgba(76, 175, 80, 0.3); border: 2px solid var(--mud-palette-success);";
        return "";
    }
    
    private RenderFragment RenderFilterMatchIndicators() => @<div>
        @if (_appliedFilters?.Cities?.Any() == true && _appliedFilters.Cities.Contains(partner.City))
        {
            <MudIcon Icon="@Icons.Material.Filled.LocationOn" Color="Color.Success" Size="Size.Small" 
                   Title="Matches location filter" />
        }
        
        @if (_appliedFilters?.Availability.HasValue)
        {
            <MudIcon Icon="@Icons.Material.Filled.Schedule" Color="Color.Success" Size="Size.Small" 
                   Title="Matches availability filter" />
        }
        
        @if (_skillMatchPercentage > 0)
        {
            <MudIcon Icon="@Icons.Material.Filled.Star" Color="Color.Success" Size="Size.Small" 
                   Title="Matches skills filter" />
        }
    </div>;
}
```

#### Results Display with Sorting and Pagination
```razor
@* Enhanced Results Section in Home.razor *@
<div class="search-results-section">
    <!-- Results Header -->
    <MudStack Direction="Row" Justify="Justify.SpaceBetween" AlignItems="Center" Class="mb-4">
        <div>
            <MudText Typo="Typo.h5">Search Results</MudText>
            <MudText Typo="Typo.body2">
                Showing @_paginatedPartners.Count of @_filteredPartners.Count partners
                @if (_activeFilterCount > 0)
                {
                    <text> matching your @_activeFilterCount filter(s)</text>
                }
            </MudText>
        </div>
        
        <!-- Sort Controls -->
        <MudStack Direction="Row" Spacing="2" AlignItems="Center">
            <MudText Typo="Typo.body2">Sort by:</MudText>
            <MudSelect T="SortOption" 
                     @bind-Value="_currentSortOption"
                     ValueChanged="OnSortChanged"
                     Variant="Variant.Outlined"
                     Style="min-width: 180px;">
                <MudSelectItem Value="SortOption.Relevance">Relevance</MudSelectItem>
                <MudSelectItem Value="SortOption.Availability">Earliest Available</MudSelectItem>
                <MudSelectItem Value="SortOption.Experience">Most Experience</MudSelectItem>
                <MudSelectItem Value="SortOption.Location">Location</MudSelectItem>
                <MudSelectItem Value="SortOption.SkillMatch">Best Skills Match</MudSelectItem>
            </MudSelect>
            
            <!-- Save Search Button -->
            @if (_isAuthenticated && _activeFilterCount > 0)
            {
                <MudButton Variant="Variant.Outlined" 
                         StartIcon="@Icons.Material.Filled.Bookmark"
                         OnClick="SaveCurrentSearch">
                    Save Search
                </MudButton>
            }
        </MudStack>
    </MudStack>
    
    <!-- Loading State -->
    @if (_isLoading)
    {
        <MudGrid>
            @for (int i = 0; i < 6; i++)
            {
                <MudItem xs="12" md="4">
                    <MudCard>
                        <MudCardHeader>
                            <CardHeaderAvatar>
                                <MudSkeleton SkeletonType="SkeletonType.Circle" Width="40px" Height="40px" />
                            </CardHeaderAvatar>
                            <CardHeaderContent>
                                <MudSkeleton SkeletonType="SkeletonType.Text" />
                                <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" />
                            </CardHeaderContent>
                        </MudCardHeader>
                        <MudCardContent>
                            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="100px" />
                        </MudCardContent>
                    </MudCard>
                </MudItem>
            }
        </MudGrid>
    }
    
    <!-- Results Grid -->
    else if (_paginatedPartners.Any())
    {
        <MudGrid>
            @foreach (var partner in _paginatedPartners)
            {
                <MudItem xs="12" md="4">
                    <PartnerCard Partner="@partner" 
                               AppliedFilters="@_appliedFilters"
                               IsFiltered="@(_activeFilterCount > 0)" />
                </MudItem>
            }
        </MudGrid>
        
        <!-- Pagination -->
        @if (_totalPages > 1)
        {
            <div class="d-flex justify-center mt-6">
                <MudPagination Count="_totalPages" 
                             @bind-Selected="_currentPage"
                             SelectedChanged="OnPageChanged"
                             ShowFirstLast="true"
                             ShowPreviousNext="true" />
            </div>
        }
    }
    
    <!-- Empty State -->
    else
    {
        <MudPaper Class="pa-8 text-center">
            <MudIcon Icon="@Icons.Material.Filled.SearchOff" Size="Size.Large" Class="mb-4" />
            <MudText Typo="Typo.h6" Class="mb-2">No partners match your criteria</MudText>
            <MudText Typo="Typo.body1" Class="mb-4">
                Try adjusting your filters or search terms to find more experts
            </MudText>
            <MudButton Variant="Variant.Filled" 
                     Color="Color.Primary"
                     OnClick="ClearAllFilters">
                Clear All Filters
            </MudButton>
        </MudPaper>
    }
</div>

@code {
    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int _pageSize = 12;
    
    public enum SortOption
    {
        Relevance,
        Availability,
        Experience,
        Location,
        SkillMatch
    }
}
```

#### Enhanced Matching Logic
```csharp
public class EnhancedMatchingService
{
    public string GenerateEnhancedMatchingReason(Partner partner, PartnerFilterCriteria? filters, string originalReason)
    {
        var reasons = new List<string> { originalReason };
        
        // Add filter-specific matching reasons
        if (filters?.Cities?.Contains(partner.City) == true)
        {
            reasons.Add($"Located in your preferred city of {partner.City}");
        }
        
        if (filters?.States?.Contains(partner.State) == true)
        {
            reasons.Add($"Based in {partner.State} as requested");
        }
        
        if (filters?.RequiredSkills?.Any() == true)
        {
            var matchingSkills = partner.Skills
                .Where(s => filters.RequiredSkills.Any(req => s.Skill.Contains(req, StringComparison.OrdinalIgnoreCase)))
                .ToList();
                
            if (matchingSkills.Any())
            {
                reasons.Add($"Expert in {string.Join(", ", matchingSkills.Take(2).Select(s => s.Skill))}");
            }
        }
        
        if (filters?.Availability.HasValue)
        {
            var availabilityText = filters.Availability switch
            {
                AvailabilityTimeframe.ThisWeek => "available this week",
                AvailabilityTimeframe.NextWeek => "available next week", 
                AvailabilityTimeframe.ThisMonth => "available this month",
                _ => "available soon"
            };
            reasons.Add($"Shows {availabilityText} as needed");
        }
        
        return string.Join(". ", reasons) + ".";
    }
    
    public List<Partner> SortPartners(List<Partner> partners, SortOption sortOption, PartnerFilterCriteria? filters = null)
    {
        return sortOption switch
        {
            SortOption.Relevance => partners, // Already sorted by AI relevance
            SortOption.Availability => partners.OrderBy(p => p.NextAvailableSlot).ToList(),
            SortOption.Experience => partners.OrderByDescending(p => p.Skills.Max(s => s.YearsOfExperience)).ToList(),
            SortOption.Location => SortByLocationProximity(partners, filters?.Cities?.FirstOrDefault()),
            SortOption.SkillMatch => SortBySkillMatch(partners, filters?.RequiredSkills),
            _ => partners
        };
    }
}
```

### Testing Requirements
```csharp
[Test]
public async Task EnhancedPartnerCard_WithFilters_DisplaysRelevantInformation()
{
    // Test enhanced card display
}

[Test]
public async Task ResultsSorting_ByAvailability_OrdersCorrectly()
{
    // Test sorting functionality
}

[Test] 
public async Task Pagination_WithLargeResultSet_WorksCorrectly()
{
    // Test pagination
}

[Test]
public async Task EmptyState_WithNoMatches_DisplaysHelpfulMessage()
{
    // Test empty state
}

[Test]
public async Task SaveSearch_WithAuthenticatedUser_SavesCorrectly()
{
    // Test save search functionality
}
```

### Definition of Done
- [ ] Enhanced partner cards implemented
- [ ] Sorting functionality working
- [ ] Pagination or infinite scroll
- [ ] Loading states and skeleton UI
- [ ] Empty state messaging
- [ ] Save search functionality
- [ ] Mobile responsive design
- [ ] Performance optimization complete

---

## Implementation Checklist

### Phase 1: Core Infrastructure ✅
- [ ] Create PartnerFilterComponent.razor basic structure
- [ ] Implement FilterService with location and skills filtering
- [ ] Add PartnerFilterCriteria data models
- [ ] Create unit tests for FilterService
- [ ] Basic integration with Home.razor

### Phase 2: Availability Integration 🔄
- [ ] Extend GoogleCalendarService for availability checking
- [ ] Implement PartnerAvailability system
- [ ] Add availability status to partner cards
- [ ] Create AvailabilityService with caching
- [ ] Add calendar integration tests

### Phase 3: Enhanced UI/UX 🔄
- [ ] Implement responsive design (desktop sidebar/mobile drawer)
- [ ] Add filter animations and transitions
- [ ] Enhance partner cards with filter information
- [ ] Implement sorting and pagination
- [ ] Add URL state management

### Phase 4: Testing & Polish 🔄
- [ ] Complete unit test coverage (>85%)
- [ ] Integration testing with real data
- [ ] Accessibility audit and fixes
- [ ] Performance optimization
- [ ] Cross-browser testing

### Success Metrics Tracking
- [ ] Set up analytics for filter usage
- [ ] Track booking conversion rates
- [ ] Monitor performance metrics
- [ ] Collect user feedback
- [ ] A/B test different filter layouts

---

*This specification provides a comprehensive implementation guide for the Advanced Partner Search feature, ensuring all user stories are properly defined with clear acceptance criteria, technical implementation details, and testing requirements.*