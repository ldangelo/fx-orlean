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
using UI.Grains.Users;
using UI.Grains.Users.Commands;
using UI.Tests.Grains.Partners;
using Whaally.Domain;
using Whaally.Domain.Abstractions;
using Whaally.Domain.Infrastructure.OrleansHost;

namespace UI.Tests.Grains.Users;
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
[TestSubject(typeof(UserAggregate))]
public class UserAggregateTests
{
    private readonly IServiceProvider _serviceProvider = DependencyContainer.Create();
    private IAggregateHandlerFactory _aggregateHandlerFactory;

     [TestMethod]
    public async Task UserAggregateTest()
    {
        _aggregateHandlerFactory = _serviceProvider.GetRequiredService<IAggregateHandlerFactory>();
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var user = _aggregateHandlerFactory.Instantiate<UserAggregate>("leo.dangelo@FortiumPartners.com");
        Assert.IsNotNull(user);

        await user.Evaluate(new SetUserDetailsCommand("leo.dangelo@FortiumPartners.com", "Leo", "D'Angelo"));
        UserSnapshot snapshot = await user.Snapshot<UserSnapshot>();

        Assert.AreEqual<String>(snapshot.firstName, "Leo");
    }
}
