using System;
using System.Threading.Tasks;
using common.Commands;
using common.Queries;
using EventServer.Aggregates.Partners;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleans.Hosting;
using UI.Aggregates.Partners;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Users;
using UI.Aggregates.Users.Commands;
using UI.Aggregates.VideoConference;
using UI.Aggregates.VideoConference.Commands;
using Xunit.DependencyInjection;

namespace UI.Tests.Aggregates.VideoConference;

public static class TestExtension
{
    public static async Task<IHost> StartServer(this IHostBuilder builder)
    {
        return await builder
            .UseOrleans(c =>
                c.UseLocalhostClustering()
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryStreams("sms")
                    .UseInMemoryReminderService()
            )
            .StartAsync();
    }
}

[Collection("Fx Collection")]
public class VideoConferenceAggregateTests : FxTest
{
    private readonly FxTestFixture _testFixture;

    public VideoConferenceAggregateTests(FxTestFixture fixture, ITestOutputHelperAccessor accessor)
        : base(accessor)
    {
        _testFixture = fixture;
    }

    [Fact]
    public async Task TestVideoConferenceGrain()
    {
        var conferenceId = Guid.NewGuid();

        var partner = _testFixture
            .getActorSystem()
            .ActorOf<PartnerAggregate>("leo.dangelo@fortiumpartners.com");
        Assert.NotNull(partner);

        var user = _testFixture.getActorSystem().ActorOf<UserAggregate>("ldangelo@mac.com");
        Assert.NotNull(user);

        await user.Tell(new CreateUserCommand("Leo", "D'Angelo", "ldangelo@mac.com"));

        await partner.Tell(
            new CreatePartnerCommand("Leo", "D'Angelo", "leo.dangelo@fortiumpartners.com")
        );
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var conference = _testFixture
            .getActorSystem()
            .ActorOf<VideoConferenceAggregate>(conferenceId.ToString());
        await conference.Tell(
            new CreateVideoConferenceCommand(
                conferenceId,
                DateTime.Now.AddDays(1).AddHours(11),
                DateTime.Now.AddDays(1).AddHours(12),
                "ldangelo@mac.com",
                "leo.dangelo@fortiumpartners.com"
            )
        );
        Assert.NotNull(conference);

        //
        // Wait for the messages to be processed.
        //
        // VideoConferenceAggregate sends out commands to the user and partner aggregates
        // This delay allows time for them to be processed so we can confirm they were delivered
        await Task.Delay(1000);

        var result = await partner.Ask<PartnerSnapshot>(new GetPartnerDetails());
        Assert.NotNull(result);
        Assert.True(result.Skills.Count == 1);
        Assert.True(result.VideoConferences.Count == 1);

        var userSnapshot = await user.Ask<UserSnapshot>(new GetUserDetails());
        Assert.NotNull(userSnapshot);
        Assert.True(userSnapshot.VideoConferences.Count == 1);
    }

    private object LoggerConfiguration()
    {
        throw new NotImplementedException();
    }
}