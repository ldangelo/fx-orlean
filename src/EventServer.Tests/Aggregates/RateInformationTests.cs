using Fortium.Types;
using Xunit;

namespace EventServer.Tests.Aggregates;

public class RateInformationTests
{
    [Fact]
    public void CalculateCost_WithBasicRate_ReturnsCorrectAmount()
    {
        // Arrange
        var rate = new RateInformation
        {
            RatePerMinute = 2.50m,
            MinimumCharge = 0m,
            MinimumMinutes = 0,
            BillingIncrementMinutes = 1
        };
        
        var startTime = DateTime.Parse("2025-01-01 10:00:00");
        var endTime = DateTime.Parse("2025-01-01 10:30:00");
        
        // Act
        var cost = rate.CalculateCost(startTime, endTime);
        
        // Assert
        Assert.Equal(75.00m, cost); // 30 minutes * $2.50
    }
    
    [Fact]
    public void CalculateCost_WithMinimumCharge_ReturnsMinimumAmount()
    {
        // Arrange
        var rate = new RateInformation
        {
            RatePerMinute = 2.50m,
            MinimumCharge = 50.00m,
            MinimumMinutes = 0,
            BillingIncrementMinutes = 1
        };
        
        var startTime = DateTime.Parse("2025-01-01 10:00:00");
        var endTime = DateTime.Parse("2025-01-01 10:10:00");
        
        // Act
        var cost = rate.CalculateCost(startTime, endTime);
        
        // Assert
        Assert.Equal(50.00m, cost); // Should use minimum charge instead of 25.00 (10 min * 2.50)
    }
    
    [Fact]
    public void CalculateCost_WithBillingIncrement_RoundsUpCorrectly()
    {
        // Arrange
        var rate = new RateInformation
        {
            RatePerMinute = 2.50m,
            MinimumCharge = 0m,
            MinimumMinutes = 0,
            BillingIncrementMinutes = 5
        };
        
        var startTime = DateTime.Parse("2025-01-01 10:00:00");
        var endTime = DateTime.Parse("2025-01-01 10:07:00");
        
        // Act
        var cost = rate.CalculateCost(startTime, endTime);
        
        // Assert
        Assert.Equal(25.00m, cost); // 7 minutes rounds up to 10 minutes (2 billing increments) * 2.50
    }
    
    [Fact]
    public void CalculateCost_WithMinimumMinutes_UsesMinimumDuration()
    {
        // Arrange
        var rate = new RateInformation
        {
            RatePerMinute = 2.50m,
            MinimumCharge = 0m,
            MinimumMinutes = 15,
            BillingIncrementMinutes = 1
        };
        
        var startTime = DateTime.Parse("2025-01-01 10:00:00");
        var endTime = DateTime.Parse("2025-01-01 10:05:00");
        
        // Act
        var cost = rate.CalculateCost(startTime, endTime);
        
        // Assert
        Assert.Equal(37.50m, cost); // Uses 15 minutes instead of actual 5 minutes
    }
    
    [Fact]
    public void CalculateCost_WithAllFeatures_CalculatesCorrectly()
    {
        // Arrange
        var rate = new RateInformation
        {
            RatePerMinute = 2.50m,
            MinimumCharge = 50.00m,
            MinimumMinutes = 15,
            BillingIncrementMinutes = 5
        };
        
        var startTime = DateTime.Parse("2025-01-01 10:00:00");
        var endTime = DateTime.Parse("2025-01-01 10:17:00");
        
        // Act
        var cost = rate.CalculateCost(startTime, endTime);
        
        // Assert
        Assert.Equal(50.00m, cost); // 17 minutes rounds up to 20 minutes = 50.00, matches minimum charge
    }
}
