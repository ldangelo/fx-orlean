using System.Threading.Tasks;
using JetBrains.Annotations;
using Orleankka.TestKit;
using UI.Aggregates.Partners.Commands;
using UI.Grains.Partners;
using UI.Grains.Users;
using Xunit;

namespace UI.Tests.Grains.Partners;

[TestSubject(typeof(PartnerAggregate))]
public class PartnerAggregateTest
{
    private readonly ActorRuntimeMock runtime;
    private readonly ActorSystemMock system;

    public PartnerAggregateTest()
    {
        runtime = new ActorRuntimeMock();
        system = runtime.System;
    }

    [Fact]
    public async Task PartnerDetailsTest()
    {
        var partner = system.MockActorOf<PartnerAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.NotNull(partner);

        var user = system.MockActorOf<UserAggregate>("ldangelo@mac.com");
        Assert.NotNull(user);

        await partner.Tell(new SetPartnerDetalsCommand("leo.dangelo@FortiumPartners.com", "Leo", "D'Angelo"));
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var partnerSnapshot = await partner.Ask<PartnerSnapshot>(partner);
        var userSnapshot = await user.Ask<UserSnapshot>(user);
        Assert.Equal("Leo", partnerSnapshot.firstName);
        Assert.True(partnerSnapshot.skills.Count == 1);
    }
}