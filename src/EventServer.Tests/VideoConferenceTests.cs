using EventServer.Aggregates.VideoConference.Commands;
using Fortium.Types;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class VideoConferenceTests : IntegrationContext
{
    public VideoConferenceTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private static RateInformation CreateDefaultRate() => new()
    {
        RatePerMinute = 2.50m,
        MinimumCharge = 25.00m,
        MinimumMinutes = 15,
        BillingIncrementMinutes = 5,
        EffectiveDate = DateTime.UtcNow.AddDays(-1),
        IsActive = true
    };

    [Fact]
    public async Task CreateVideoConference()
    {
        var command = new CreateVideoConferenceCommand(
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddHours(1),
            "test@fortiumpartners.com",
            "test@google.com",
            CreateDefaultRate()
        );

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });
    }
}
