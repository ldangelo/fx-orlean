using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Hosting;
using Orleans.Serialization;
using Orleans.TestingHost;
using UI.Aggregates.Partners.Commands;
using UI.Grains.Partners;
using UI.Grains.Users;
using Whaally.Domain;
using Whaally.Domain.Abstractions;
using Whaally.Domain.Infrastructure.OrleansHost;

namespace UI.Tests.Grains.Partners;

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(static services =>
        {
            services.AddDomain();
            services.Remove(services.Single(q => q.ServiceType == typeof(IAggregateHandlerFactory)));
            services.AddSingleton<IAggregateHandlerFactory, OrleansAggregateHandlerFactory>();

            services.AddMarten(config =>
            {
                config.Connection(Environment.GetEnvironmentVariable("MARTEN_CONNECTION_STRING"));
            });
            services.AddSerializer(serializer => serializer.AddProtobufSerializer());
        }).AddCustomStorageBasedLogConsistencyProvider();
    }
}

[TestClass]
[TestSubject(typeof(PartnerAggregate))]
public class PartnerAggregateTest
{
    private readonly IServiceProvider _serviceProvider = DependencyContainer.Create();
    private IAggregateHandlerFactory _aggregateHandlerFactory;

    [TestMethod]
    public async Task PartnerDetailsTest()
    {
        _aggregateHandlerFactory = _serviceProvider.GetRequiredService<IAggregateHandlerFactory>();
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var partner = _aggregateHandlerFactory.Instantiate<PartnerAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(partner);

        var user = _aggregateHandlerFactory.Instantiate<UserAggregate>("ldangelo@mac.com");
        Assert.IsNotNull(user);

        var result =
            await partner.Evaluate(new SetPartnerDetalsCommand("leo.dangelo@FortiumPartners.com", "Leo", "D'Angelo"));
        var result2 = await partner.Evaluate(new AddPartnerSkillCommand("AWS"));

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result2.IsSuccess);

        var partnerSnapshot = await partner.Snapshot<PartnerSnapshot>();
        var userSnapshot = await user.Snapshot<UserSnapshot>();
        Assert.AreEqual(partnerSnapshot.firstName, "Leo");
        Assert.IsTrue(partnerSnapshot.skills.Count == 1);
    }
}