using System.Threading.Tasks;
using EventServer.Aggregates.Partners.Commands;

namespace EventServer.Tests.Aggregates.Partners;

public class PartnerAggregateTest : IntegrationContext
{
    public PartnerAggregateTest(AppFixture fixture)
        : base(fixture) { }

    [Fact]
    public Task CreatePartner()
    {
        CreatePartnerCommand command = new CreatePartnerCommand(
            "Leo",
            "D'Angelo",
            "leo.dangelo@fortiumpartners.com"
        );

        Assert.True(true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task PartnerDetailsTest()
    {
        Assert.True(true);
    }
}
