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
using Serilog;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.VideoConference;
using UI.Grains.Partners;
using UI.Grains.Users;
using UI.Grains.Users.Commands;
using UI.Grains.VideoConference;
using UI.Services;
using UI.Tests.Grains.Partners;
using Weasel.Core;
using Whaally.Domain;
using Whaally.Domain.Abstractions;
using Whaally.Domain.Infrastructure.OrleansHost;

namespace UI.Tests.Grains.VideoConference;

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(static services =>
            {
                services.AddDomain("UI.Tests");
                services.Remove(services.Single(q => q.ServiceType == typeof(IAggregateHandlerFactory)));
                services.AddSingleton<IAggregateHandlerFactory, OrleansAggregateHandlerFactory>();

                var martenBuilder = services.AddMarten(config =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("MARTEN_CONNECTION_STRING");
                    Log.Information($"Marten connection string: {connectionString}");
                    if (connectionString == null) throw new InvalidOperationException();
                    config.Connection(connectionString);
                    config.AutoCreateSchemaObjects = AutoCreate.All;
                    config.Events.MetadataConfig.EnableAll();
                });
                martenBuilder.OptimizeArtifactWorkflow();

                services.AddSerializer(serializer => serializer.AddProtobufSerializer());
            }).AddActivityPropagation()
            .AddCustomStorageBasedLogConsistencyProvider()
            .AddMemoryGrainStorage("conference");
    }
}

[TestClass]
[TestSubject(typeof(VideoConferenceAggregate))]
public class VideoConferenceAggregateTests
{
    private readonly IServiceProvider _serviceProvider = DependencyContainer.Create();
    private IAggregateHandlerFactory _aggregateHandlerFactory;

    [TestMethod]
    public async Task TestVideoConferenceGrain()
    {
        _aggregateHandlerFactory = _serviceProvider.GetRequiredService<IAggregateHandlerFactory>();
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        var cluster = builder.Build();
        cluster.Deploy();

        var conferenceId = Guid.NewGuid();
        var user = _aggregateHandlerFactory.Instantiate<UserAggregate>("ldangelo@mac.com");
        var partner = _aggregateHandlerFactory.Instantiate<PartnerAggregate>("leo.dangelo@fortiumpartners.com");

        await user.Evaluate(new SetUserDetailsCommand("ldangelo@mac.com", "Leo", "D'Angelo"));
        await partner.Evaluate(new SetPartnerDetalsCommand("leo.dangelo@fortiumpartners.com", "Leo", "D'Angelo"));

        //
        // call the conferenceservice and create the conference
        var services = DependencyContainer.Create();
        var context = new ServiceHandlerContext(services, new ServiceMetadata());
        var service = new VideoConferenceService(
            conferenceId.ToString(),
            "ldangelo@mac.com",
            "leo.dangelo@fortiumpartners.com",
            DateTime.Now,
            DateTime.Now
        );

        var serviceResult = await new VideoConferenceServiceHandler(_aggregateHandlerFactory).Handle(context, service);
        Assert.IsTrue(serviceResult.IsSuccess);
        var conference = _aggregateHandlerFactory.Instantiate<VideoConferenceAggregate>(conferenceId.ToString());
        var snapshot = conference.Snapshot<VideoConferenceSnapshot>();


        // ensure the conference was created
        Assert.IsNotNull(snapshot);

        var userSnapshot = await user.Snapshot<UserSnapshot>();
        var partnerSnapshot = await partner.Snapshot<PartnerSnapshot>();

        // ensure the conference was added to the user and the partner
        Assert.IsTrue(partnerSnapshot.videoConferences.Count == 1);
        Assert.IsTrue(userSnapshot.videoConferences.Count == 1);
    }
}