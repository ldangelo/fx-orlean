using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Partners;

namespace UI.Tests.Grains.Partners;

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

[TestSubject(typeof(PartnerAggregate))]
public class PartnerAggregateTest
{
    private static IActorSystem _system;

    [Fact]
    public async Task PartnerDetailsTest()
    {
        var host = new HostBuilder()
            .UseOrleans(c => c.UseLocalhostClustering())
            .UseOrleankka()
            .Build();

        await host.StartAsync();
        _system = host.ActorSystem();

        var partner = _system.ActorOf<PartnerAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.NotNull(partner);

        await partner.Tell(new CreatePartnerCommand("Leo", "D'Angelo", "leo.dangelo@fortiumpartners.com"));
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var partnerSnapshot = await partner.Ask<PartnerSnapshot>(new GetPartnerDetails());
        Assert.True(partnerSnapshot.skills.Count == 1);
    }
}
