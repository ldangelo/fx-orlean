using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Hosting;
using Orleans.TestingHost;
using UI.Grains.Interfaces;

namespace UI.Tests.Grains.Users;

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.AddMemoryGrainStorage("users");
    }
}
[TestClass]
[TestSubject(typeof(IUsersGrain))]
public class UserGrainTests
{
    [TestMethod]
    public async Task PartnerDetailsTest()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var user = cluster.GrainFactory.GetGrain<IUsersGrain>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(user);

        await user.SetUserDetails("Leo", "D'Angelo", "leo.dangelo@FortiumPartners.com");


        Assert.AreEqual<String>(await user.getFirstName(), "Leo");
    }
}
