using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using org.fortium.fx.common;
using Orleankka.Cluster;
using Orleans.Hosting;
using Weasel.Core;

namespace EventServer.Tests;

public class Startup
{
    private IHost _host;

    public async void ConfigureHost(IHostBuilder builder)
    {
        _host = new HostBuilder()
            .UseOrleans(c =>
            {
                c.UseLocalhostClustering();
                // streaming
                c.AddMemoryStreams("sms");
                c.AddMemoryStreams("conferences");
                c.AddMemoryStreams("partners");
                c.AddMemoryStreams("users");

                // storage
                c.AddMartenGrainStorageAsDefault();
                c.AddMemoryGrainStorage("sms");
                c.AddMemoryGrainStorage("PubSubStore");
                c.AddMartenGrainStorage("partners");
                c.AddMartenGrainStorage("conferences");
                c.AddMartenGrainStorage("users");
            })
            .UseOrleankka()
            .ConfigureServices((builderContext, services) => { ConfigureServices(builderContext, services); })
            .Build();

        await _host.StartAsync();
    }

    public void ConfigureServices(HostBuilderContext builder, IServiceCollection services)
    {
        services.AddSingleton<IActorSystem>(_host.ActorSystem());
        services
            .AddMarten(options =>
            {
                options.Connection(
                    "Host=localhost;Port=5432;Database=fx-expert-test;Username=postgres;Password=itsasecret;"
                );
                options.UseNewtonsoftForSerialization();

                options.AutoCreateSchemaObjects = AutoCreate.All;

                options.Schema.For<PartnerSnapshot>();
            })
            .OptimizeArtifactWorkflow();
    }
}
