using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Users.Commands;
using UI.Aggregates.VideoConference;
using UI.Aggregates.Partners;
using UI.Aggregates.Users;
using UI.Aggregates.VideoConference.Commands;

namespace UI.Tests.Aggregates.VideoConference;

public static class TestExtension
{
    public static async Task<IHost> StartServer(this IHostBuilder builder)
    {
        return await builder
            .UseOrleans(c => c
                .UseLocalhostClustering()
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryStreams("sms")
                .UseInMemoryReminderService())
            .StartAsync();
    }
}

public class VideoConferenceAggregateTests
{
    private static IActorSystem _system;

    [Fact]
    public async Task TestVideoConferenceGrain()
    {
        var host = new HostBuilder()
            .UseOrleans(c => c.UseLocalhostClustering())
            .UseOrleankka()
            .Build();

        await host.StartAsync();
        _system = host.ActorSystem();
        var conferenceId = Guid.NewGuid();

        var partner = _system.ActorOf<PartnerAggregate>("leo.dangelo@fortiumpartners.com");
        Assert.NotNull(partner);

        var user = _system.ActorOf<UserAggregate>("ldangelo@mac.com");
        Assert.NotNull(user);

        await user.Tell(new CreateUserCommand("Leo", "D'Angelo", "ldangelo@mac.com"));

        await partner.Tell(new CreatePartnerCommand("leo.dangelo@fortiumpartners.com", "Leo", "D'Angelo"));
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var conference = _system.ActorOf<VideoConferenceAggregate>(conferenceId.ToString());
        await conference.Tell(new CreateVideoConferenceCommand(conferenceId, DateTime.Now, DateTime.Now,
            "ldangelo@mac.com",
            "leo.dangelo@fortiumpartners.com"));
        Assert.NotNull(conference);

        //
        // Wait for the messages to be processed.
        //
        // VideoConferenceAggregate sends out commands to the user and partner aggregates
        // This delay allows time for them to be processed so we can confirm they were delivered
        await Task.Delay(1000);

        var result = await partner.Ask<PartnerSnapshot>(new GetPartnerDetails());
        Assert.NotNull(result);
        Assert.True(result.skills.Count == 1);
        Assert.True(result.videoConferences.Count == 1);

        var userSnapshot = await user.Ask<UserSnapshot>(new GetUserDetails());
        Assert.NotNull(userSnapshot);
        Assert.True(userSnapshot.VideoConferences.Count == 1);
    }
}
