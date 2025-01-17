using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using Serilog;
using UI.Aggregates.Users;
using UI.Aggregates.Users.Commands;
using Xunit.DependencyInjection;

namespace UI.Tests.Aggregates.Users;

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

[TestSubject(typeof(UserAggregate))]
[Collection("Fx Collection")]
public class UserAggregateTests: FxTest
{
    private FxTestFixture _testFixture;

    public UserAggregateTests(FxTestFixture fixture,ITestOutputHelperAccessor accessor): base(accessor)
    {
        _testFixture = fixture;
    }

    [Fact]
    public async Task UserAggregateTest()
    {
        var user = _testFixture
            .getActorSystem()
            .ActorOf<IUserAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.NotNull(user);

        await user.Tell(
            new CreateUserCommand("Leo", "D'Angelo", "leo.dangelo@FortiumPartners.com")
        );

        var snapshot = await user.Ask<UserSnapshot>(new GetUserDetails());

        Assert.NotNull(snapshot);
        Assert.Equal("Leo", snapshot.FirstName);
    }
}
