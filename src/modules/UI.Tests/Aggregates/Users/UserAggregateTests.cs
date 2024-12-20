using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using UI.Aggregates.Users.Commands;
using UI.Grains.Users;
using Xunit;

namespace UI.Tests.Aggregates.Users;

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

[TestSubject(typeof(UserAggregate))]
public class UserAggregateTests
{
    private static IActorSystem _system;

    [Fact]
    public async Task UserAggregateTest()
    {
        var host = new HostBuilder()
            .UseOrleans(c => c.UseLocalhostClustering())
            .UseOrleankka()
            .Build();

        await host.StartAsync();
        _system = host.ActorSystem();

        var user = _system.ActorOf<IUserAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.NotNull(user);

        await user.Tell(new CreateUserCommand("Leo", "D'Angelo", "leo.dangelo@FortiumPartners.com"));

        var snapshot = await user.Ask<UserSnapshot>(new GetUserDetails());

        Assert.NotNull(snapshot);
        Assert.Equal("Leo", snapshot.firstName);
    }
}