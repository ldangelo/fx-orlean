using System;
using System.Threading.Tasks;
using EventServer.Aggregates.Partners;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using org.fortium.fx.common;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using Orleans.Serialization;
using Serilog;
using Weasel.Core;

namespace EventServer.Tests;

public class FxTestFixture : IAsyncLifetime
{
    private readonly IHost Host;

    public LoggerConfiguration Logger;

    public FxTestFixture()
    {
        Host = new HostBuilder()
            .UseOrleans(c =>
            {
                c.Services.AddSerializer(b =>
                    b.AddNewtonsoftJsonSerializer(t => t.Namespace.StartsWith("shared_types.Types"))
                );
                c.UseLocalhostClustering();
                c.AddMemoryGrainStorageAsDefault();
                c.AddMemoryGrainStorage("PubSubStore");
                c.AddMemoryStreams("sms");
                c.AddMemoryStreams("videoconferences");
                c.AddMemoryStreams("partners");
                c.AddMemoryStreams("users");

                c.AddMemoryGrainStorage("sms");
                c.AddMemoryGrainStorage("partners");
            })
            .UseOrleankka()
            .ConfigureServices(
                (builderContext, services) =>
                {
                    // configure Martendb
                    services
                        .AddMarten(c =>
                        {
                            c.Connection(
                                "Host=localhost;Port=5432;Database=fx-expert-test;Username=postgres;Password=itsasecret;"
                            );
                            c.UseNewtonsoftForSerialization();
                            //                            c.Events.DatabaseSchemaName = "events";
                            c.Schema.For<PartnerSnapshot>();
                            c.AutoCreateSchemaObjects = AutoCreate.All;
                            c.Events.StreamIdentity = StreamIdentity.AsString;
                        })
                        .OptimizeArtifactWorkflow();
                }
            )
            .Build();

        Host.StartAsync();
    }

    public void Dispose() { }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IActorSystem getActorSystem()
    {
        return Host.Services.GetService<IActorSystem>();
    }

    public async Task InitializeAsync()
    {
        var store = Host.Services.GetRequiredService<IDocumentStore>();

        await store.Advanced.ResetAllData();
    }
}

[CollectionDefinition("Fx Collection")]
public class FxTestCollection : ICollectionFixture<FxTestFixture> { }
