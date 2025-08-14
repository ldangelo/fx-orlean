using EventServer.Services;
using Shouldly;
using Xunit;

namespace EventServer.Tests.Services;

public class RoleAssignmentServiceTests
{
    [Fact]
    public void DetermineUserRole_WithFortiumPartnersEmail_ShouldReturnPartnerRole()
    {
        // Arrange
        var email = "john.doe@fortiumpartners.com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("PARTNER");
    }

    [Fact]
    public void DetermineUserRole_WithFortiumPartnersEmailUppercase_ShouldReturnPartnerRole()
    {
        // Arrange
        var email = "admin@FORTIUMPARTNERS.COM";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("PARTNER");
    }

    [Fact]
    public void DetermineUserRole_WithFortiumPartnersEmailMixedCase_ShouldReturnPartnerRole()
    {
        // Arrange
        var email = "consultant@FortiumPartners.Com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("PARTNER");
    }

    [Fact]
    public void DetermineUserRole_WithSubdomainFortiumPartnersEmail_ShouldReturnClientRole()
    {
        // Arrange - Subdomains should not be considered Fortium partners
        var email = "user@subdomain.fortiumpartners.com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Theory]
    [InlineData("user@gmail.com")]
    [InlineData("business@company.com")]
    [InlineData("contact@startup.io")]
    [InlineData("admin@clientcompany.com")]
    [InlineData("someone@fortium.com")]
    [InlineData("user@notfortiumpartners.com")]
    public void DetermineUserRole_WithNonFortiumEmail_ShouldReturnClientRole(string email)
    {
        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithNullEmail_ShouldReturnClientRole()
    {
        // Arrange
        string email = null;

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithEmptyEmail_ShouldReturnClientRole()
    {
        // Arrange
        var email = "";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithWhitespaceEmail_ShouldReturnClientRole()
    {
        // Arrange
        var email = "   ";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Theory]
    [InlineData("invalid@email")]
    [InlineData("@fortiumpartners.com")]
    [InlineData("user@")]
    [InlineData("notanemail")]
    public void DetermineUserRole_WithInvalidEmailFormat_ShouldReturnClientRole(string email)
    {
        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithMultipleAtSymbols_ShouldReturnClientRole()
    {
        // Arrange
        var email = "user@@fortiumpartners.com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithFortiumPartnersInUsername_ShouldReturnClientRole()
    {
        // Arrange - Domain should be checked, not username
        var email = "fortiumpartners@gmail.com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_WithPartialFortiumDomain_ShouldReturnClientRole()
    {
        // Arrange - Should be exact domain match
        var email = "user@fortiumpartner.com";

        // Act
        var result = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result.ShouldBe("CLIENT");
    }

    [Fact]
    public void DetermineUserRole_IsIdempotent()
    {
        // Arrange
        var email = "test@fortiumpartners.com";

        // Act
        var result1 = RoleAssignmentService.DetermineUserRole(email);
        var result2 = RoleAssignmentService.DetermineUserRole(email);

        // Assert
        result1.ShouldBe(result2);
        result1.ShouldBe("PARTNER");
    }

    [Fact]
    public void DetermineUserRole_PerformanceTest_ShouldBeEfficient()
    {
        // Arrange
        var emails = new[]
        {
            "user1@fortiumpartners.com",
            "user2@gmail.com",
            "user3@fortiumpartners.com",
            "user4@company.com"
        };

        // Act & Assert - Should complete quickly for multiple calls
        var startTime = DateTime.UtcNow;
        
        for (int i = 0; i < 1000; i++)
        {
            foreach (var email in emails)
            {
                RoleAssignmentService.DetermineUserRole(email);
            }
        }
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        
        duration.TotalMilliseconds.ShouldBeLessThan(100); // Should complete in under 100ms
    }
}