using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EventServer.Aggregates.Payments.Commands;
using EventServer.Aggregates.VideoConference.Commands;
using Fortium.Types;
using Xunit;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class PaymentControllerTests : IntegrationContext
{
    public PaymentControllerTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task AuthorizePayment_ShouldReturnPaymentIntentId()
    {
        // Arrange - First create a video conference
        var createConferenceCommand = new CreateVideoConferenceCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "user@example.com",
            "partner@example.com",
            new RateInformation 
            {
                RatePerMinute = 1.5m, 
                MinimumCharge = 10.0m, 
                MinimumMinutes = 15, 
                BillingIncrementMinutes = 5,
                EffectiveDate = DateTime.UtcNow, 
                IsActive = true
            }
        );

        var conferenceResponse = await Scenario(x =>
        {
            x.Post.Json(createConferenceCommand).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Now create the payment authorization command with the existing conference
        var command = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            createConferenceCommand.ConferenceId,
            100.0m,
            "USD",
            "user@example.com",
            new RateInformation 
            {
                RatePerMinute = 1.5m, 
                MinimumCharge = 10.0m, 
                MinimumMinutes = 15, 
                BillingIncrementMinutes = 5,
                EffectiveDate = DateTime.UtcNow, 
                IsActive = true
            }
        );

        // Act
        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(201);
        });
    }

    [Fact]
    public async Task CapturePayment_ShouldReturnSuccess()
    {
        // Arrange - First create a video conference
        var createConferenceCommand = new CreateVideoConferenceCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "user@example.com",
            "partner@example.com",
            new RateInformation 
            {
                RatePerMinute = 1.5m, 
                MinimumCharge = 10.0m, 
                MinimumMinutes = 15, 
                BillingIncrementMinutes = 5,
                EffectiveDate = DateTime.UtcNow, 
                IsActive = true
            }
        );

        var conferenceResponse = await Scenario(x =>
        {
            x.Post.Json(createConferenceCommand).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Create authorization command with existing conference
        var authorizeCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            createConferenceCommand.ConferenceId,
            100.0m,
            "USD",
            "user@example.com",
            new RateInformation 
            {
                RatePerMinute = 1.5m, 
                MinimumCharge = 10.0m, 
                MinimumMinutes = 15, 
                BillingIncrementMinutes = 5,
                EffectiveDate = DateTime.UtcNow, 
                IsActive = true
            }
        );
        var authorizeResponse = await Scenario(x =>
        {
            x.Post.Json(authorizeCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(201);
        });

        var captureCommand = new CapturePaymentCommand(authorizeCommand.PaymentId.ToString(), DateTime.Now);

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(captureCommand).ToUrl($"/payments/capture/{captureCommand.PaymentIntentId}");
            x.StatusCodeShouldBe(204);
        });
    }
}
