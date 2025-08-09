using EventServer.Services;
using Shouldly;
using Xunit.Abstractions;

namespace EventServer.Tests.Services;

public class EmailServiceTests
{
    private readonly ITestOutputHelper _output;

    public EmailServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BookingEmailResult_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var result = new BookingEmailResult();

        // Assert
        result.Success.ShouldBeFalse();
        result.ClientEmailSent.ShouldBeFalse();
        result.PartnerEmailSent.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void BookingEmailResult_WithSuccessData_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var result = new BookingEmailResult
        {
            Success = true,
            ClientEmailSent = true,
            PartnerEmailSent = true
        };

        // Assert
        result.Success.ShouldBeTrue();
        result.ClientEmailSent.ShouldBeTrue();
        result.PartnerEmailSent.ShouldBeTrue();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void BookingEmailResult_WithPartialFailure_ShouldReflectActualState()
    {
        // Arrange & Act
        var result = new BookingEmailResult
        {
            Success = false,
            ClientEmailSent = true,
            PartnerEmailSent = false,
            ErrorMessage = "Partner email delivery failed"
        };

        // Assert
        result.Success.ShouldBeFalse();
        result.ClientEmailSent.ShouldBeTrue();
        result.PartnerEmailSent.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Partner email delivery failed");
    }

    // Note: Integration tests for EmailService are covered in BookingIntegrationTests
    // Unit tests would require complex SMTP mocking which is better handled through integration testing
}