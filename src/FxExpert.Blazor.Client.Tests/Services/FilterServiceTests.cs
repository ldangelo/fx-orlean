using FxExpert.Blazor.Client.Models;
using FxExpert.Blazor.Client.Services;
using Fortium.Types;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace FxExpert.Blazor.Client.Tests.Services;

public class FilterServiceTests
{
    private readonly Mock<ICalendarHttpService> _mockCalendarService;
    private readonly FilterService _filterService;
    private readonly List<Partner> _testPartners;

    public FilterServiceTests()
    {
        // Create mock CalendarHttpService interface
        _mockCalendarService = new Mock<ICalendarHttpService>();
        
        // Create FilterService with the mocked CalendarHttpService
        _filterService = new FilterService(_mockCalendarService.Object);
        
        _testPartners = CreateTestPartners();
    }

    #region Test Data Setup

    private static List<Partner> CreateTestPartners()
    {
        return new List<Partner>
        {
            new Partner
            {
                FirstName = "John",
                LastName = "Smith",
                EmailAddress = "john.smith@example.com",
                City = "San Francisco",
                State = "CA",
                Country = "USA",
                Title = "Senior CTO",
                Rate = 800.00,
                AvailabilityNext30Days = 5,
                Skills = new List<PartnerSkill>
                {
                    new("Azure", 8, ExperienceLevel.Expert),
                    new("C#", 10, ExperienceLevel.Expert),
                    new("Leadership", 12, ExperienceLevel.Expert)
                }
            },
            new Partner
            {
                FirstName = "Jane",
                LastName = "Doe",
                EmailAddress = "jane.doe@example.com",
                City = "New York",
                State = "NY",
                Country = "USA",
                Title = "CISO",
                Rate = 900.00,
                AvailabilityNext30Days = 3,
                Skills = new List<PartnerSkill>
                {
                    new("Security", 7, ExperienceLevel.Expert),
                    new("AWS", 5, ExperienceLevel.Proficient),
                    new("Risk Management", 9, ExperienceLevel.Expert)
                }
            },
            new Partner
            {
                FirstName = "Bob",
                LastName = "Johnson",
                EmailAddress = "bob.johnson@example.com",
                City = "Chicago",
                State = "IL",
                Country = "USA",
                Title = "CIO",
                Rate = 750.00,
                AvailabilityNext30Days = 8,
                Skills = new List<PartnerSkill>
                {
                    new("Project Management", 6, ExperienceLevel.Proficient),
                    new("Digital Transformation", 4, ExperienceLevel.Proficient),
                    new("Strategy", 8, ExperienceLevel.Expert)
                }
            },
            new Partner
            {
                FirstName = "Alice",
                LastName = "Wilson",
                EmailAddress = "", // Partner without email
                City = "Austin",
                State = "TX",
                Country = "USA",
                Title = "VP Engineering",
                Rate = 700.00,
                AvailabilityNext30Days = 2,
                Skills = new List<PartnerSkill>
                {
                    new("Node.js", 5, ExperienceLevel.Proficient),
                    new("React", 4, ExperienceLevel.Proficient)
                }
            },
            new Partner
            {
                FirstName = "David",
                LastName = "Brown",
                EmailAddress = "david.brown@example.com",
                City = "Seattle",
                State = "WA",
                Country = "USA",
                Title = "Technical Director",
                Rate = 850.00,
                AvailabilityNext30Days = 6,
                Skills = new List<PartnerSkill>
                {
                    new("Python", 9, ExperienceLevel.Expert),
                    new("Machine Learning", 6, ExperienceLevel.Expert),
                    new("Data Architecture", 7, ExperienceLevel.Expert)
                }
            }
        };
    }

    #endregion

    #region FilterPartnersAsync Tests

    [Fact]
    public async Task FilterPartnersAsync_WhenPartnersListIsNull_ReturnsEmptyList()
    {
        // Arrange
        var criteria = new PartnerFilterCriteria();

        // Act
        var result = await _filterService.FilterPartnersAsync(null, criteria);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task FilterPartnersAsync_WhenPartnersListIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var partners = new List<Partner>();
        var criteria = new PartnerFilterCriteria();

        // Act
        var result = await _filterService.FilterPartnersAsync(partners, criteria);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task FilterPartnersAsync_WithNoCriteria_ReturnsAllPartners()
    {
        // Arrange
        var criteria = new PartnerFilterCriteria();

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(_testPartners.Count);
    }

    [Theory]
    [InlineData("San Francisco", 1)]
    [InlineData("New York", 1)]
    [InlineData("NonExistent", 0)]
    public async Task FilterPartnersAsync_FilterByCity_ReturnsCorrectResults(string city, int expectedCount)
    {
        // Arrange
        var criteria = new PartnerFilterCriteria
        {
            Cities = new List<string> { city }
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(expectedCount);
        if (expectedCount > 0)
        {
            result.All(p => p.City.Equals(city, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData("CA", 1)]
    [InlineData("NY", 1)]
    [InlineData("IL", 1)]
    [InlineData("NonExistent", 0)]
    public async Task FilterPartnersAsync_FilterByState_ReturnsCorrectResults(string state, int expectedCount)
    {
        // Arrange
        var criteria = new PartnerFilterCriteria
        {
            States = new List<string> { state }
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(expectedCount);
        if (expectedCount > 0)
        {
            result.All(p => p.State.Equals(state, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData("west-coast", 2)] // CA, WA
    [InlineData("east-coast", 1)] // NY
    [InlineData("midwest", 1)]    // IL
    [InlineData("south", 1)]      // TX
    public async Task FilterPartnersAsync_FilterByRegion_ReturnsCorrectResults(string region, int expectedCount)
    {
        // Arrange
        var criteria = new PartnerFilterCriteria
        {
            Regions = new List<string> { region }
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(expectedCount);
    }

    [Theory]
    [InlineData(AvailabilityTimeframe.ThisWeek, 3)]   // >= 5 availability
    [InlineData(AvailabilityTimeframe.NextWeek, 4)]  // >= 3 availability
    [InlineData(AvailabilityTimeframe.ThisMonth, 5)] // >= 1 availability
    public async Task FilterPartnersAsync_FilterByAvailability_ReturnsCorrectResults(
        AvailabilityTimeframe timeframe, int expectedCount)
    {
        // Arrange
        var criteria = new PartnerFilterCriteria
        {
            Availability = timeframe
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(expectedCount);
    }

    [Fact]
    public async Task FilterPartnersAsync_FilterByRequiredSkills_ReturnsOnlyMatchingPartners()
    {
        // Arrange - Find partners with both Azure and C# skills
        var criteria = new PartnerFilterCriteria
        {
            RequiredSkills = new List<string> { "Azure", "C#" }
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(1); // Only John Smith has both Azure and C#
        result[0].FirstName.ShouldBe("John");
        result[0].LastName.ShouldBe("Smith");
    }

    [Fact]
    public async Task FilterPartnersAsync_FilterByMinExperienceLevel_ReturnsCorrectPartners()
    {
        // Arrange - Find partners with Expert level skills
        var criteria = new PartnerFilterCriteria
        {
            MinExperienceLevel = ExperienceLevel.Expert
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(4); // All except Alice who only has Proficient level
        result.All(p => p.Skills.Any(s => s.ExperienceLevel >= ExperienceLevel.Expert)).ShouldBeTrue();
    }

    [Fact]
    public async Task FilterPartnersAsync_FilterByMinYearsExperience_ReturnsCorrectPartners()
    {
        // Arrange - Find partners with at least 8 years experience in any skill
        var criteria = new PartnerFilterCriteria
        {
            MinYearsExperience = 8
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(4); // John, Jane, Bob, David have skills with 8+ years
        result.All(p => p.Skills.Any(s => s.YearsOfExperience >= 8)).ShouldBeTrue();
    }

    [Fact]
    public async Task FilterPartnersAsync_CombinedFilters_AppliesAllCriteria()
    {
        // Arrange - Complex filter: West Coast + Expert level + Security skills
        var criteria = new PartnerFilterCriteria
        {
            Regions = new List<string> { "west-coast" },
            MinExperienceLevel = ExperienceLevel.Expert,
            RequiredSkills = new List<string> { "Python" }
        };

        // Act
        var result = await _filterService.FilterPartnersAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(1); // Only David meets all criteria
        result[0].FirstName.ShouldBe("David");
        result[0].State.ShouldBe("WA"); // West coast
        result[0].Skills.Any(s => s.ExperienceLevel >= ExperienceLevel.Expert).ShouldBeTrue();
        result[0].Skills.Any(s => s.Skill.Contains("Python")).ShouldBeTrue();
    }

    #endregion

    #region RefreshPartnerAvailabilityAsync Tests

    [Fact]
    public async Task RefreshPartnerAvailabilityAsync_WhenPartnersEmpty_ReturnsEmptyList()
    {
        // Arrange
        var partners = new List<Partner>();

        // Act
        var result = await _filterService.RefreshPartnerAvailabilityAsync(partners);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task RefreshPartnerAvailabilityAsync_WhenAllPartnersHaveEmails_CallsCalendarService()
    {
        // Arrange
        var partners = _testPartners.Where(p => !string.IsNullOrEmpty(p.EmailAddress)).ToList();
        var availabilityData = new Dictionary<string, int>
        {
            { "john.smith@example.com", 7 },
            { "jane.doe@example.com", 4 },
            { "bob.johnson@example.com", 9 }
        };

        _mockCalendarService
            .Setup(x => x.RefreshMultiplePartnerAvailabilityAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(availabilityData);

        // Act
        var result = await _filterService.RefreshPartnerAvailabilityAsync(partners);

        // Assert
        result.Count.ShouldBe(partners.Count);
        _mockCalendarService.Verify(x => x.RefreshMultiplePartnerAvailabilityAsync(
            It.Is<List<string>>(emails => emails.Count == partners.Count),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify availability was updated
        result.First(p => p.EmailAddress == "john.smith@example.com").AvailabilityNext30Days.ShouldBe(7);
        result.First(p => p.EmailAddress == "jane.doe@example.com").AvailabilityNext30Days.ShouldBe(4);
        result.First(p => p.EmailAddress == "bob.johnson@example.com").AvailabilityNext30Days.ShouldBe(9);
    }

    [Fact]
    public async Task RefreshPartnerAvailabilityAsync_WhenCalendarServiceThrows_ReturnsOriginalPartners()
    {
        // Arrange
        var partners = _testPartners.Take(2).ToList();
        _mockCalendarService
            .Setup(x => x.RefreshMultiplePartnerAvailabilityAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Calendar service unavailable"));

        var originalAvailability = partners[0].AvailabilityNext30Days;

        // Act
        var result = await _filterService.RefreshPartnerAvailabilityAsync(partners);

        // Assert
        result.Count.ShouldBe(partners.Count);
        result[0].AvailabilityNext30Days.ShouldBe(originalAvailability); // Should remain unchanged
    }

    [Fact]
    public async Task RefreshPartnerAvailabilityAsync_WithPartnersWithoutEmails_SkipsThosePartners()
    {
        // Arrange
        var partners = _testPartners.ToList(); // Includes Alice who has no email
        var availabilityData = new Dictionary<string, int>
        {
            { "john.smith@example.com", 10 },
            { "jane.doe@example.com", 5 }
        };

        _mockCalendarService
            .Setup(x => x.RefreshMultiplePartnerAvailabilityAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(availabilityData);

        // Act
        var result = await _filterService.RefreshPartnerAvailabilityAsync(partners);

        // Assert
        result.Count.ShouldBe(partners.Count);
        
        // Partners with emails should have updated availability
        result.First(p => p.EmailAddress == "john.smith@example.com").AvailabilityNext30Days.ShouldBe(10);
        
        // Partners without emails should keep original availability
        var aliceResult = result.First(p => p.FirstName == "Alice");
        aliceResult.AvailabilityNext30Days.ShouldBe(2); // Original value
    }

    #endregion

    #region ApplyRealTimeAvailabilityFilterAsync Tests

    [Fact]
    public async Task ApplyRealTimeAvailabilityFilterAsync_WhenNoAvailabilityCriteria_ReturnsAllPartners()
    {
        // Arrange
        var criteria = new PartnerFilterCriteria(); // No availability specified

        // Act
        var result = await _filterService.ApplyRealTimeAvailabilityFilterAsync(_testPartners, criteria);

        // Assert
        result.Count.ShouldBe(_testPartners.Count);
    }

    [Fact]
    public async Task ApplyRealTimeAvailabilityFilterAsync_WithAvailabilityCheck_CallsCalendarService()
    {
        // Arrange
        var partnersWithEmails = _testPartners.Where(p => !string.IsNullOrEmpty(p.EmailAddress)).ToList();
        var criteria = new PartnerFilterCriteria
        {
            Availability = AvailabilityTimeframe.ThisWeek
        };

        _mockCalendarService
            .Setup(x => x.IsPartnerAvailableInTimeframeAsync(It.IsAny<string>(), It.IsAny<AvailabilityTimeframe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _filterService.ApplyRealTimeAvailabilityFilterAsync(partnersWithEmails, criteria);

        // Assert
        result.Count.ShouldBe(partnersWithEmails.Count); // All should be included since we return true
        _mockCalendarService.Verify(x => x.IsPartnerAvailableInTimeframeAsync(
            It.IsAny<string>(),
            AvailabilityTimeframe.ThisWeek,
            It.IsAny<CancellationToken>()), Times.Exactly(partnersWithEmails.Count));
    }

    [Fact]
    public async Task ApplyRealTimeAvailabilityFilterAsync_WhenPartnerNotAvailable_ExcludesFromResults()
    {
        // Arrange
        var partnersWithEmails = _testPartners.Where(p => !string.IsNullOrEmpty(p.EmailAddress)).Take(2).ToList();
        var criteria = new PartnerFilterCriteria
        {
            Availability = AvailabilityTimeframe.ThisWeek
        };

        // Mock: First partner available, second not available
        _mockCalendarService.SetupSequence(x => x.IsPartnerAvailableInTimeframeAsync(It.IsAny<string>(), It.IsAny<AvailabilityTimeframe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        // Act
        var result = await _filterService.ApplyRealTimeAvailabilityFilterAsync(partnersWithEmails, criteria);

        // Assert
        result.Count.ShouldBe(1); // Only first partner should be included
        result[0].EmailAddress.ShouldBe(partnersWithEmails[0].EmailAddress);
    }

    [Fact]
    public async Task ApplyRealTimeAvailabilityFilterAsync_WhenCalendarServiceThrows_FallsBackToStaticData()
    {
        // Arrange
        var partnersWithEmails = _testPartners.Where(p => !string.IsNullOrEmpty(p.EmailAddress)).ToList();
        var criteria = new PartnerFilterCriteria
        {
            Availability = AvailabilityTimeframe.ThisWeek // Requires >= 5 availability
        };

        _mockCalendarService
            .Setup(x => x.IsPartnerAvailableInTimeframeAsync(It.IsAny<string>(), It.IsAny<AvailabilityTimeframe>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var result = await _filterService.ApplyRealTimeAvailabilityFilterAsync(partnersWithEmails, criteria);

        // Assert
        // Should fallback to static availability data
        // Partners with AvailabilityNext30Days >= 5 should be included
        var expectedCount = partnersWithEmails.Count(p => p.AvailabilityNext30Days >= 5);
        result.Count.ShouldBe(expectedCount);
    }

    [Fact]
    public async Task ApplyRealTimeAvailabilityFilterAsync_SkipsPartnersWithoutEmails()
    {
        // Arrange
        var allPartners = _testPartners.ToList(); // Includes Alice without email
        var criteria = new PartnerFilterCriteria
        {
            Availability = AvailabilityTimeframe.ThisWeek
        };

        _mockCalendarService
            .Setup(x => x.IsPartnerAvailableInTimeframeAsync(It.IsAny<string>(), It.IsAny<AvailabilityTimeframe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _filterService.ApplyRealTimeAvailabilityFilterAsync(allPartners, criteria);

        // Assert
        // Only partners with emails should be checked
        var partnersWithEmails = allPartners.Where(p => !string.IsNullOrEmpty(p.EmailAddress)).Count();
        result.Count.ShouldBe(partnersWithEmails);
        result.All(p => !string.IsNullOrEmpty(p.EmailAddress)).ShouldBeTrue();
    }

    #endregion

    #region Region and Utility Method Tests

    [Theory]
    [InlineData("CA", "west-coast")]
    [InlineData("California", "west-coast")]
    [InlineData("WA", "west-coast")]
    [InlineData("Washington", "west-coast")]
    [InlineData("NY", "east-coast")]
    [InlineData("New York", "east-coast")]
    [InlineData("MA", "east-coast")]
    [InlineData("IL", "midwest")]
    [InlineData("Illinois", "midwest")]
    [InlineData("OH", "midwest")]
    [InlineData("TX", "south")]
    [InlineData("Texas", "south")]
    [InlineData("FL", "south")]
    [InlineData("Unknown", "other")]
    [InlineData("", "other")]
    [InlineData(null, "other")]
    public void GetPartnerRegion_ReturnsCorrectRegion(string? state, string expectedRegion)
    {
        // Act
        var result = _filterService.GetPartnerRegion(state);

        // Assert
        result.ShouldBe(expectedRegion);
    }

    [Fact]
    public async Task SearchCitiesAsync_WithEmptyQuery_ReturnsAllCitiesLimited()
    {
        // Act
        var result = await _filterService.SearchCitiesAsync(_testPartners, "", 3);

        // Assert
        result.Count.ShouldBeLessThanOrEqualTo(3);
        result.All(city => _testPartners.Any(p => p.City == city)).ShouldBeTrue();
    }

    [Fact]
    public async Task SearchCitiesAsync_WithSpecificQuery_ReturnsMatchingCities()
    {
        // Act
        var result = await _filterService.SearchCitiesAsync(_testPartners, "San", 5);

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldBe("San Francisco");
    }

    [Fact]
    public async Task SearchSkillsAsync_WithEmptyQuery_ReturnsAllSkillsLimited()
    {
        // Act
        var result = await _filterService.SearchSkillsAsync(_testPartners, "", 5);

        // Assert
        result.Count.ShouldBeLessThanOrEqualTo(5);
        var allSkills = _testPartners.SelectMany(p => p.Skills.Select(s => s.Skill)).Distinct().ToList();
        result.All(skill => allSkills.Contains(skill)).ShouldBeTrue();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithSpecificQuery_ReturnsMatchingSkills()
    {
        // Act
        var result = await _filterService.SearchSkillsAsync(_testPartners, "C#", 5);

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldBe("C#");
    }

    [Fact]
    public void GetAvailableStates_ReturnsAllUSStates()
    {
        // Act
        var result = _filterService.GetAvailableStates(_testPartners);

        // Assert
        result.Count.ShouldBe(50); // All US states
        result.ShouldContain(s => s.Code == "CA" && s.Name == "California");
        result.ShouldContain(s => s.Code == "NY" && s.Name == "New York");
        result.ShouldContain(s => s.Code == "TX" && s.Name == "Texas");
    }

    #endregion

    #region SortPartners Tests

    [Theory]
    [InlineData(PartnerSortOption.Name, true, "Alice")]  // Ascending by name
    [InlineData(PartnerSortOption.Name, false, "John")]  // Descending by name
    public void SortPartners_ByName_SortsCorrectly(PartnerSortOption sortBy, bool ascending, string expectedFirstName)
    {
        // Act
        var result = _filterService.SortPartners(_testPartners, sortBy, ascending);

        // Assert
        result[0].FirstName.ShouldBe(expectedFirstName);
    }

    [Theory]
    [InlineData(PartnerSortOption.Availability, true, 2)]   // Ascending - Alice has lowest (2)
    [InlineData(PartnerSortOption.Availability, false, 8)]  // Descending - Bob has highest (8)
    public void SortPartners_ByAvailability_SortsCorrectly(PartnerSortOption sortBy, bool ascending, int expectedAvailability)
    {
        // Act
        var result = _filterService.SortPartners(_testPartners, sortBy, ascending);

        // Assert
        result[0].AvailabilityNext30Days.ShouldBe(expectedAvailability);
    }

    [Fact]
    public void SortPartners_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Partner>();

        // Act
        var result = _filterService.SortPartners(emptyList, PartnerSortOption.Name, true);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void SortPartners_WithNullList_ReturnsEmptyList()
    {
        // Act
        var result = _filterService.SortPartners(null, PartnerSortOption.Name, true);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetFilterStats Tests

    [Fact]
    public void GetFilterStats_WithActiveFilters_CalculatesCorrectStats()
    {
        // Arrange
        var allPartners = _testPartners;
        var filteredPartners = _testPartners.Take(2).ToList(); // 40% reduction
        var criteria = new PartnerFilterCriteria
        {
            Cities = new List<string> { "San Francisco" },
            Availability = AvailabilityTimeframe.ThisWeek,
            RequiredSkills = new List<string> { "Azure" }
        };

        // Act
        var result = _filterService.GetFilterStats(allPartners, filteredPartners, criteria);

        // Assert
        result.TotalPartners.ShouldBe(5);
        result.FilteredPartners.ShouldBe(2);
        result.ActiveFilters.ShouldBe(3); // City, Availability, Skills
        result.FilterReduction.ShouldBe(60.0); // (5-2)/5 * 100
    }

    [Fact]
    public void GetFilterStats_WithNoFilters_ShowsZeroActiveFilters()
    {
        // Arrange
        var allPartners = _testPartners;
        var filteredPartners = _testPartners;
        var criteria = new PartnerFilterCriteria();

        // Act
        var result = _filterService.GetFilterStats(allPartners, filteredPartners, criteria);

        // Assert
        result.TotalPartners.ShouldBe(5);
        result.FilteredPartners.ShouldBe(5);
        result.ActiveFilters.ShouldBe(0);
        result.FilterReduction.ShouldBe(0.0);
    }

    [Fact]
    public void GetFilterStats_WithEmptyResults_CalculatesCorrectReduction()
    {
        // Arrange
        var allPartners = _testPartners;
        var filteredPartners = new List<Partner>();
        var criteria = new PartnerFilterCriteria
        {
            RequiredSkills = new List<string> { "NonExistentSkill" }
        };

        // Act
        var result = _filterService.GetFilterStats(allPartners, filteredPartners, criteria);

        // Assert
        result.TotalPartners.ShouldBe(5);
        result.FilteredPartners.ShouldBe(0);
        result.ActiveFilters.ShouldBe(1);
        result.FilterReduction.ShouldBe(100.0); // Complete reduction
    }

    #endregion
}