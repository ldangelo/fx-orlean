using Alba;
using EventServer.Aggregates.VideoConference;
using EventServer.Aggregates.VideoConference.Commands;
using Fortium.Types;
using Xunit;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class VideoConferenceRateTests : IntegrationContext
{
    public VideoConferenceRateTests(AppFixture fixture, ITestOutputHelper output)
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
    public async Task CreateVideoConference_WithValidRate_Succeeds()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(1);
        var rate = CreateValidRate();

        var command = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Verify the conference was created with rate
        var result = await Scenario(x =>
        {
            x.Get.Url($"/videos/{conferenceId}");
            x.StatusCodeShouldBe(200);
        });

        var content = result.ReadAsJson<VideoConferenceState>();
        Assert.Equal(rate.RatePerMinute, content.RateInformation.RatePerMinute);
    }

    [Fact]
    public async Task CreateVideoConference_WithInvalidRate_Fails()
    {
        // Arrange
        var command = new CreateVideoConferenceCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "test@fortiumpartners.com",
            "test@google.com",
            new RateInformation
            {
                RatePerMinute = -1, // Invalid rate
                MinimumCharge = 25.00m,
                MinimumMinutes = 15,
                BillingIncrementMinutes = 5,
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            }
        );

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(400);
        });
    }

    [Fact]
    public async Task CreateVideoConference_RateExpirationBeforeEndTime_Fails()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(2);
        var rate = CreateValidRate();
        rate.ExpirationDate = startTime.AddMinutes(30); // Expires before conference ends

        var command = new CreateVideoConferenceCommand(
            Guid.NewGuid(),
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(400);
        });
    }

    [Fact]
    public async Task GetVideoConference_IncludesEstimatedCost()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddMinutes(30);
        var rate = CreateValidRate();

        var command = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        // Create the conference
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Verify the estimated cost
        var result = await Scenario(x =>
        {
            x.Get.Url($"/videos/{conferenceId}");
            x.StatusCodeShouldBe(200);
        });

        var conference = result.ReadAsJson<VideoConferenceState>();
        Assert.Equal(75.00m, conference.EstimatedCost); // 30 minutes at $2.50/min
    }

    [Fact]
    public async Task CreateVideoConference_WithMinimumDuration_CalculatesCostCorrectly()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddMinutes(10); // Less than minimum duration
        var rate = CreateValidRate(); // Has 15 minute minimum

        var command = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        // Create the conference
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Verify cost uses minimum duration
        var result = await Scenario(x =>
        {
            x.Get.Url($"/videos/{conferenceId}");
            x.StatusCodeShouldBe(200);
        });

        var conference = result.ReadAsJson<VideoConferenceState>();
        Assert.Equal(37.50m, conference.EstimatedCost); // 15 minutes at $2.50/min
    }

    [Fact]
    public async Task CreateVideoConference_WithBillingIncrement_RoundsUpCost()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddMinutes(17); // Will round up to 20 minutes
        var rate = CreateValidRate(); // Has 5 minute billing increments

        var command = new CreateVideoConferenceCommand(
            conferenceId,
            startTime,
            endTime,
            "test@fortiumpartners.com",
            "test@google.com",
            rate
        );

        // Create the conference
        await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

        // Verify cost is rounded up
        var result = await Scenario(x =>
        {
            x.Get.Url($"/videos/{conferenceId}");
            x.StatusCodeShouldBe(200);
        });

        var conference = result.ReadAsJson<VideoConferenceState>();
        Assert.Equal(50.00m, conference.EstimatedCost); // 20 minutes at $2.50/min
    }
}
