using System;
using System.Threading.Tasks;
using Orleankka.TestKit;
using UI.Aggregates.Partners.Commands;
using UI.Grains.Partners;
using Xunit;

namespace UI.Tests.Aggregates.VideoConference;

public class VideoConferenceAggregateTests
{
    private readonly ActorRuntimeMock runtime;
    private readonly ActorSystemMock system;

    public VideoConferenceAggregateTests()
    {
        runtime = new ActorRuntimeMock();
        system = runtime.System;
    }

    [Fact]
    public async Task TestVideoConferenceGrain()
    {
        var conferenceId = Guid.NewGuid();
        var partner = system.MockActorOf<PartnerAggregate>("leo.dangelo@fortiumpartners.com");
        Assert.NotNull(partner);

        await partner.Tell(new CreatePartnerCommand("leo.dangelo@fortiumpartners.com", "Leo", "D'Angelo"));
        await partner.Tell(new AddPartnerSkillCommand("AWS"));

        var result = await partner.Ask<PartnerSnapshot>(partner);
        Assert.NotNull(result);
        Assert.True(result.skills.Count == 1);
    }
}