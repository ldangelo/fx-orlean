using Alba;
using EventServer.Aggregates.Payments;
using EventServer.Aggregates.Payments.Commands;
using EventServer.Aggregates.VideoConference;
using EventServer.Aggregates.VideoConference.Commands;
using Fortium.Types;
using Xunit;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class ConferencePaymentTests : IntegrationContext
{
    public ConferencePaymentTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private static RateInformation CreateValidRate() => new()
    {
        RatePerMinute = 2.50m,
        MinimumCharge = 25.00m,
        MinimumMinutes = 15,
        BillingIncrementMinutes = 5,
        EffectiveDate = DateTime.UtcNow.AddDays(-1),
        IsActive = true
    };

    [Fact]
    public async Task AuthorizePayment_ForValidConference_Succeeds()
    {
        // First create a conference
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(1);
        var rate = CreateValidRate();

        var conferenceCommand = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(conferenceCommand).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Now authorize payment
        var paymentId = Guid.NewGuid();
        var paymentCommand = new AuthorizeConferencePaymentCommand(
            paymentId,
            conferenceId,
            150.00m, // 60 minutes * $2.50
            "USD",
            "test@fortiumpartners.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(paymentCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(201);
        });

        // Verify payment was authorized
        var result = await Scenario(x =>
        {
            x.Get.Url($"/payments/{paymentId}");
            x.StatusCodeShouldBe(200);
        });

        var payment = result.ReadAsJson<PaymentState>();
        Assert.Equal("Authorized", payment.Status);
    }

    [Fact]
    public async Task AuthorizePayment_WithInvalidAmount_Fails()
    {
        var conferenceId = Guid.NewGuid();
        var rate = CreateValidRate();

        var paymentCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            conferenceId,
            0m, // Invalid amount
            "USD",
            "test@fortiumpartners.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(paymentCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(400);
        });
    }

    [Fact]
    public async Task AuthorizePayment_ForNonexistentConference_Fails()
    {
        var rate = CreateValidRate();

        var paymentCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(), // Non-existent conference
            150.00m,
            "USD",
            "test@fortiumpartners.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(paymentCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task AuthorizePayment_AmountMatchesEstimatedCost()
    {
        // Create a conference
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddMinutes(30);
        var rate = CreateValidRate();

        var conferenceCommand = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(conferenceCommand).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Get the conference to verify estimated cost
        var confResult = await Scenario(x =>
        {
            x.Get.Url($"/videos/{conferenceId}");
            x.StatusCodeShouldBe(200);
        });

        var conference = confResult.ReadAsJson<VideoConferenceState>();
        var estimatedCost = conference.EstimatedCost;

        // Authorize payment with matching amount
        var paymentCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            conferenceId,
            estimatedCost,
            "USD",
            "test@fortiumpartners.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(paymentCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(201);
        });
    }

    [Fact]
    public async Task AuthorizePayment_WithInvalidCurrency_Fails()
    {
        var conferenceId = Guid.NewGuid();
        var rate = CreateValidRate();

        var paymentCommand = new AuthorizeConferencePaymentCommand(
            Guid.NewGuid(),
            conferenceId,
            150.00m,
            "INVALID", // Invalid currency code
            "test@fortiumpartners.com",
            rate
        );

        await Scenario(x =>
        {
            x.Post.Json(paymentCommand).ToUrl("/payments/authorize");
            x.StatusCodeShouldBe(400);
        });
    }
}
