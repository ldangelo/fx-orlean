using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Hosting;
using Orleans.TestingHost;
using UI.Grains.Partners;
using UI.Grains.Interfaces;

namespace UI.Tests.Grains.Partners;

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.AddMemoryGrainStorage("partners");
    }
}
[TestClass]
[TestSubject(typeof(PartnerGrain))]
public class PartnerGrainTest
{

    [TestMethod]
    public async Task PartnerDetailsTest()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var partner = cluster.GrainFactory.GetGrain<IPartnerGrain>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(partner);

        await partner.SetPartnerDetails("leo.dangelo@FortiumPartners.com", "Leo", "D'Angelo");
        await partner.AddSkill("AWS");


        Assert.AreEqual<String>(await partner.GetFirstName(), "Leo");
        var skills = await partner.GetSkills();
        Assert.IsTrue(skills.Count == 1);
    }
}
