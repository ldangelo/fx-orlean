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
        var command = new AuthorizePaymentCommand(100.0m, "USD", "pm_card_visa", DateTime.Now);

        // Act
        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(200);
        });

        var result = initial.ReadAsJson<Payment>();
        Assert.NotNull(result?.PaymentId);
    }

    [Fact]
    public async Task CapturePayment_ShouldReturnSuccess()
    {
        // Arrange
        var authorizeCommand = new AuthorizePaymentCommand(
            100.0m,
            "USD",
            "pm_card_visa",
            DateTime.Now
        );
        var authorizeResponse = await Scenario(x =>
        {
            x.Post.Json(authorizeCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(200);
        });

        var captureCommand = new CapturePaymentCommand("pm_card_visa", DateTime.Now);

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(captureCommand).ToUrl("/payments/capture/pm_card_visa");
            x.StatusCodeShouldBe(204);
        });

        // Assert
    }
}
