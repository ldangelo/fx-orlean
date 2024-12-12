using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Hosting;
using Orleans.TestingHost;
using UI.Grains.Interfaces;
using System.Threading.Tasks;

namespace UI.Tests.Grains.VideoConference;

internal sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.AddMemoryGrainStorage("partners");
        siloBuilder.AddMemoryGrainStorage("users");
        siloBuilder.AddMemoryGrainStorage("conferences");
    }
}
[TestClass]
[TestSubject(typeof(IVideoConference))]
public class VideoConferenceGrainTest
{
    [TestMethod]
    public async Task TestVideoConferenceGrain()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<VideoConference.TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var partner = cluster.GrainFactory.GetGrain<IPartnerGrain>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(partner);

        var user = cluster.GrainFactory.GetGrain<IUsersGrain>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(user);

        await user.SetUserDetails("Leo", "D'Angelo", "leo.dangelo@FortiumPartners.com");

        var conferenceId = Guid.NewGuid();
        var conference = cluster.GrainFactory.GetGrain<IVideoConference>(conferenceId);

        Assert.IsNotNull(conference);

        await conference.CreateConference(partner, user, new DateTime(2025, 01, 01), new DateTime(2025, 02, 01));

        // ensure the conference was added to the user and the partner
        Assert.IsTrue((await user.getVideoConferences()).Count > 0);
        Assert.IsTrue((await partner.getVideoConferences()).Count > 0);
    }

}
