using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EventServer.Aggregates.Payments.Commands;
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
        // Arrange
        var command = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100.0m,
            "USD",
            "user@example.com",
            new RateInformation { RatePerMinute = 1.5m, MinimumCharge = 10.0m }
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
        // Arrange
        var authorizeCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100.0m,
            "USD",
            "user@example.com",
            new RateInformation { RatePerMinute = 1.5m, MinimumCharge = 10.0m }
        );
        var authorizeResponse = await Scenario(x =>
        {
            x.Post.Json(authorizeCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(201);
        });

        var captureCommand = new CapturePaymentCommand(Guid.NewGuid().ToString(), DateTime.Now);

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(captureCommand).ToUrl($"/payments/capture/{captureCommand.PaymentIntentId}");
            x.StatusCodeShouldBe(204);
        });
    }
}
