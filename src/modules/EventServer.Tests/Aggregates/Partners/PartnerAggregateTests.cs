using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleans.Hosting;
using UI.Aggregates.Partners;
using UI.Aggregates.Partners.Commands;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace UI.Tests.Grains.Partners;

[TestSubject(typeof(PartnerAggregate))]
[Collection("Fx Collection")]
public class PartnerAggregateTest : FxTest
{
    FxTestFixture fixture;

    public PartnerAggregateTest(FxTestFixture _fixture, ITestOutputHelperAccessor accessor)
        : base(accessor)
    {
        fixture = _fixture;
    }

    [Fact]
    public async Task PartnerDetailsTest()
    {
        var partner = fixture
            .getActorSystem()
            .ActorOf<PartnerAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.NotNull(partner);

        await partner.Tell(
            new CreatePartnerCommand("Leo", "D'Angelo", "leo.dangelo@fortiumpartners.com")
        );
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var partnerSnapshot = await partner.Ask<PartnerSnapshot>(new GetPartnerDetails());
        Assert.True(partnerSnapshot.skills.Count == 1);
    }
}
