using FxExpert.Blazor.Client.Models;
using Fortium.Types;

namespace FxExpert.Blazor.Client.Services;

/// <summary>
/// Service for client-side partner filtering and search operations
/// </summary>
public class FilterService
{
    /// <summary>
    /// Filters partners based on the provided criteria
    /// </summary>
    /// <param name="partners">List of partners to filter</param>
    /// <param name="criteria">Filter criteria to apply</param>
    /// <returns>Filtered list of partners</returns>
    public async Task<List<Partner>> FilterPartnersAsync(List<Partner> partners, PartnerFilterCriteria criteria)
    {
        if (partners == null || !partners.Any())
            return new List<Partner>();

        await Task.Delay(1); // Ensure async behavior
        
        var filtered = partners.AsEnumerable();
        
        // Apply location filters
        filtered = ApplyLocationFilters(filtered, criteria);
        
        // Apply availability filters
        filtered = ApplyAvailabilityFilters(filtered, criteria);
        
        // Apply skills and experience filters
        filtered = ApplySkillsFilters(filtered, criteria);
        
        return filtered.ToList();
    }

    /// <summary>
    /// Applies location-based filters (city, state, region)
    /// </summary>
    private IEnumerable<Partner> ApplyLocationFilters(IEnumerable<Partner> partners, PartnerFilterCriteria criteria)
    {
        var filtered = partners;
        
        // Filter by cities
        if (criteria.Cities?.Any() == true)
        {
            filtered = filtered.Where(p => 
                criteria.Cities.Contains(p.City, StringComparer.OrdinalIgnoreCase));
        }
        
        // Filter by states
        if (criteria.States?.Any() == true)
        {
            filtered = filtered.Where(p => 
                criteria.States.Contains(p.State, StringComparer.OrdinalIgnoreCase));
        }
        
        // Filter by regions
        if (criteria.Regions?.Any() == true)
        {
            filtered = filtered.Where(p => 
                criteria.Regions.Contains(GetPartnerRegion(p.State)));
        }
        
        return filtered;
    }

    /// <summary>
    /// Applies availability-based filters
    /// </summary>
    private IEnumerable<Partner> ApplyAvailabilityFilters(IEnumerable<Partner> partners, PartnerFilterCriteria criteria)
    {
        if (!criteria.Availability.HasValue)
            return partners;

        // For now, this is based on the AvailabilityNext30Days field
        // In the future, this could integrate with Google Calendar API for real-time availability
        return criteria.Availability switch
        {
            AvailabilityTimeframe.ThisWeek => partners.Where(p => p.AvailabilityNext30Days >= 5), // High availability
            AvailabilityTimeframe.NextWeek => partners.Where(p => p.AvailabilityNext30Days >= 3), // Medium availability
            AvailabilityTimeframe.ThisMonth => partners.Where(p => p.AvailabilityNext30Days >= 1), // Some availability
            _ => partners
        };
    }

    /// <summary>
    /// Applies skills and experience-based filters
    /// </summary>
    private IEnumerable<Partner> ApplySkillsFilters(IEnumerable<Partner> partners, PartnerFilterCriteria criteria)
    {
        var filtered = partners;
        
        // Filter by required skills (AND logic - partner must have ALL selected skills)
        if (criteria.RequiredSkills?.Any() == true)
        {
            filtered = filtered.Where(partner =>
                criteria.RequiredSkills.All(requiredSkill =>
                    partner.Skills?.Any(partnerSkill =>
                        partnerSkill.Skill.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase)) == true));
        }
        
        // Filter by minimum experience level
        if (criteria.MinExperienceLevel.HasValue)
        {
            filtered = filtered.Where(partner =>
                partner.Skills?.Any(skill => skill.ExperienceLevel >= criteria.MinExperienceLevel) == true);
        }
        
        // Filter by minimum years of experience
        if (criteria.MinYearsExperience.HasValue)
        {
            filtered = filtered.Where(partner =>
                partner.Skills?.Any(skill => skill.YearsOfExperience >= criteria.MinYearsExperience) == true);
        }
        
        return filtered;
    }

    /// <summary>
    /// Determines the region for a given state
    /// </summary>
    /// <param name="state">State code or name</param>
    /// <returns>Region identifier</returns>
    public string GetPartnerRegion(string? state)
    {
        if (string.IsNullOrEmpty(state)) return "other";
        
        var westCoastStates = new[] { "CA", "OR", "WA", "AZ", "NV", "California", "Oregon", "Washington", "Arizona", "Nevada" };
        var eastCoastStates = new[] { "NY", "NJ", "CT", "MA", "ME", "NH", "VT", "RI", "DE", "MD", "New York", "New Jersey", "Connecticut", "Massachusetts", "Maine", "New Hampshire", "Vermont", "Rhode Island", "Delaware", "Maryland" };
        var midwestStates = new[] { "IL", "IN", "IA", "KS", "MI", "MN", "MO", "NE", "ND", "OH", "SD", "WI", "Illinois", "Indiana", "Iowa", "Kansas", "Michigan", "Minnesota", "Missouri", "Nebraska", "North Dakota", "Ohio", "South Dakota", "Wisconsin" };
        var southStates = new[] { "AL", "AR", "FL", "GA", "KY", "LA", "MS", "NC", "SC", "TN", "TX", "VA", "WV", "Alabama", "Arkansas", "Florida", "Georgia", "Kentucky", "Louisiana", "Mississippi", "North Carolina", "South Carolina", "Tennessee", "Texas", "Virginia", "West Virginia" };
        
        if (westCoastStates.Contains(state, StringComparer.OrdinalIgnoreCase)) return "west-coast";
        if (eastCoastStates.Contains(state, StringComparer.OrdinalIgnoreCase)) return "east-coast";
        if (midwestStates.Contains(state, StringComparer.OrdinalIgnoreCase)) return "midwest";
        if (southStates.Contains(state, StringComparer.OrdinalIgnoreCase)) return "south";
        return "other";
    }

    /// <summary>
    /// Searches for cities based on the provided query
    /// </summary>
    /// <param name="partners">Partners to extract cities from</param>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <returns>List of matching cities</returns>
    public async Task<List<string>> SearchCitiesAsync(List<Partner> partners, string? query, int limit = 10)
    {
        await Task.Delay(50); // Small delay to simulate search
        
        var availableCities = partners
            .Where(p => !string.IsNullOrEmpty(p.City))
            .Select(p => p.City!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        
        if (string.IsNullOrWhiteSpace(query))
            return availableCities.Take(limit).ToList();
            
        return availableCities
            .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Searches for skills based on the provided query
    /// </summary>
    /// <param name="partners">Partners to extract skills from</param>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <returns>List of matching skills</returns>
    public async Task<List<string>> SearchSkillsAsync(List<Partner> partners, string? query, int limit = 15)
    {
        await Task.Delay(50); // Small delay to simulate search
        
        var availableSkills = partners
            .SelectMany(p => p.Skills?.Select(s => s.Skill) ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();
        
        if (string.IsNullOrWhiteSpace(query))
            return availableSkills.Take(limit).ToList();
            
        return availableSkills
            .Where(s => s.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Gets all unique states from the provided partners
    /// </summary>
    /// <param name="partners">Partners to extract states from</param>
    /// <returns>List of state information</returns>
    public List<StateInfo> GetAvailableStates(List<Partner> partners)
    {
        // Get all US states for the dropdown
        var allStates = new List<StateInfo>
        {
            new("Alabama", "AL"), new("Alaska", "AK"), new("Arizona", "AZ"), new("Arkansas", "AR"),
            new("California", "CA"), new("Colorado", "CO"), new("Connecticut", "CT"), new("Delaware", "DE"),
            new("Florida", "FL"), new("Georgia", "GA"), new("Hawaii", "HI"), new("Idaho", "ID"),
            new("Illinois", "IL"), new("Indiana", "IN"), new("Iowa", "IA"), new("Kansas", "KS"),
            new("Kentucky", "KY"), new("Louisiana", "LA"), new("Maine", "ME"), new("Maryland", "MD"),
            new("Massachusetts", "MA"), new("Michigan", "MI"), new("Minnesota", "MN"), new("Mississippi", "MS"),
            new("Missouri", "MO"), new("Montana", "MT"), new("Nebraska", "NE"), new("Nevada", "NV"),
            new("New Hampshire", "NH"), new("New Jersey", "NJ"), new("New Mexico", "NM"), new("New York", "NY"),
            new("North Carolina", "NC"), new("North Dakota", "ND"), new("Ohio", "OH"), new("Oklahoma", "OK"),
            new("Oregon", "OR"), new("Pennsylvania", "PA"), new("Rhode Island", "RI"), new("South Carolina", "SC"),
            new("South Dakota", "SD"), new("Tennessee", "TN"), new("Texas", "TX"), new("Utah", "UT"),
            new("Vermont", "VT"), new("Virginia", "VA"), new("Washington", "WA"), new("West Virginia", "WV"),
            new("Wisconsin", "WI"), new("Wyoming", "WY")
        };
        
        return allStates;
    }

    /// <summary>
    /// Sorts partners based on the provided criteria
    /// </summary>
    /// <param name="partners">Partners to sort</param>
    /// <param name="sortBy">Sort criteria</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Sorted list of partners</returns>
    public List<Partner> SortPartners(List<Partner> partners, PartnerSortOption sortBy, bool ascending = true)
    {
        if (partners == null || !partners.Any())
            return new List<Partner>();

        var sorted = sortBy switch
        {
            PartnerSortOption.Name => ascending 
                ? partners.OrderBy(p => p.GetFullName()) 
                : partners.OrderByDescending(p => p.GetFullName()),
            PartnerSortOption.Location => ascending 
                ? partners.OrderBy(p => p.City).ThenBy(p => p.State) 
                : partners.OrderByDescending(p => p.City).ThenByDescending(p => p.State),
            PartnerSortOption.Experience => ascending 
                ? partners.OrderBy(p => p.Skills?.Max(s => s.YearsOfExperience) ?? 0) 
                : partners.OrderByDescending(p => p.Skills?.Max(s => s.YearsOfExperience) ?? 0),
            PartnerSortOption.Availability => ascending 
                ? partners.OrderBy(p => p.AvailabilityNext30Days) 
                : partners.OrderByDescending(p => p.AvailabilityNext30Days),
            PartnerSortOption.Rate => ascending 
                ? partners.OrderBy(p => p.Rate) 
                : partners.OrderByDescending(p => p.Rate),
            PartnerSortOption.Relevance => partners.OrderBy(p => p.rank), // AI-determined rank
            _ => partners.OrderBy(p => p.GetFullName())
        };

        return sorted.ToList();
    }

    /// <summary>
    /// Calculates filter statistics for the given partners and criteria
    /// </summary>
    /// <param name="allPartners">All available partners</param>
    /// <param name="filteredPartners">Partners after filtering</param>
    /// <param name="criteria">Applied filter criteria</param>
    /// <returns>Filter statistics</returns>
    public PartnerFilterStats GetFilterStats(List<Partner> allPartners, List<Partner> filteredPartners, PartnerFilterCriteria criteria)
    {
        var activeFilters = 0;
        
        // Count active location filters
        if (!string.IsNullOrEmpty(criteria.Cities?.FirstOrDefault())) activeFilters++;
        if (!string.IsNullOrEmpty(criteria.States?.FirstOrDefault())) activeFilters++;
        if (criteria.Regions?.Any() == true) activeFilters++;
        
        // Count availability filter
        if (criteria.Availability.HasValue) activeFilters++;
        
        // Count skills filters
        if (criteria.RequiredSkills?.Any() == true) activeFilters++;
        if (criteria.MinExperienceLevel.HasValue) activeFilters++;
        if (criteria.MinYearsExperience.HasValue && criteria.MinYearsExperience > 0) activeFilters++;

        return new PartnerFilterStats
        {
            TotalPartners = allPartners.Count,
            FilteredPartners = filteredPartners.Count,
            ActiveFilters = activeFilters,
            FilterReduction = allPartners.Count > 0 
                ? (double)(allPartners.Count - filteredPartners.Count) / allPartners.Count * 100 
                : 0
        };
    }
}

/// <summary>
/// Options for sorting partners
/// </summary>
public enum PartnerSortOption
{
    Name,
    Location,
    Experience,
    Availability,
    Rate,
    Relevance // AI-determined ranking
}

/// <summary>
/// Statistics about filter application
/// </summary>
public class PartnerFilterStats
{
    public int TotalPartners { get; set; }
    public int FilteredPartners { get; set; }
    public int ActiveFilters { get; set; }
    public double FilterReduction { get; set; }
}