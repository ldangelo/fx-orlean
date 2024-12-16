using System;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using JasperFx.Core;
using JetBrains.Annotations;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Namotion.Reflection;
using Orleans.Hosting;
using Orleans.Serialization;
using Orleans.TestingHost;
using UI.Aggregates.Partners.Commands;
using UI.Grains.Partners;
using UI.Grains.Interfaces;
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
[TestSubject(typeof(Partner))]
public class PartnerTest
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

        var partner = _aggregateHandlerFactory.Instantiate<Partner>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(partner);

        var result = await partner.Evaluate(new SetPartnerDetalsCommand("leo.dangelo@FortiumPartners.com", "Leo", "D'Angelo"));
        var result2 = await partner.Evaluate(new AddPartnerSkillCommand("AWS"));

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result2.IsSuccess);

        PartnerSnapshot snapshot = await partner.Snapshot<PartnerSnapshot>();
        Assert.AreEqual<String>(snapshot.firstName, "Leo");
        var skills = snapshot.skills;
        Assert.IsTrue(skills.Count == 1);
    }
}