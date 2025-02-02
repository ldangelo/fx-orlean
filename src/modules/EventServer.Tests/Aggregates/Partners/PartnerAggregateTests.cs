using System.Threading.Tasks;
using common.Commands;
using common.Queries;
using EventServer.Aggregates.Partners;
using JetBrains.Annotations;
using Orleankka;
using UI.Aggregates.Partners;
using UI.Aggregates.Partners.Commands;
using Xunit.DependencyInjection;

namespace UI.Tests.Grains.Partners;

[TestSubject(typeof(PartnerAggregate))]
[Collection("Fx Collection")]
public class PartnerAggregateTest : FxTest
{
    private readonly FxTestFixture fixture;

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

        //
        // The primary phone comes from partner connect
        Assert.NotNull(partnerSnapshot.PrimaryPhone);
        Assert.True(partnerSnapshot.Skills.Count == 1);
    }
}