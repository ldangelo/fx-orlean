using System.Threading.Tasks;
using common.Commands;
using common.Queries;
using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Partners.Commands;
using JetBrains.Annotations;
using Orleankka;
using Xunit.DependencyInjection;

namespace EventServer.Tests.Aggregates.Partners;

[TestSubject(typeof(PartnerAggregate))]
[Collection("Fx Collection")]
public class PartnerAggregateTest : FxTest
{
    private readonly FxTestFixture _fixture;

    public PartnerAggregateTest(FxTestFixture fixture, ITestOutputHelperAccessor accessor)
        : base(accessor)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PartnerDetailsTest()
    {
        var partner = _fixture
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