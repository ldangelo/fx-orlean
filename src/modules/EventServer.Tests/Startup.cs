using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using Serilog;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

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
                c.AddMemoryStreams("sms");
                c.AddMemoryGrainStorage("sms");
                c.AddMemoryStreams("conferences");
                c.AddMemoryGrainStorage("conferences");
                c.AddMemoryStreams("partner");
                c.AddMemoryGrainStorage("partner");
                c.AddMemoryStreams("users");
                c.AddMemoryGrainStorage("users");
            })
            .UseOrleankka()
            .Build();

        await _host.StartAsync();
    }

    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton<IActorSystem>(_host.ActorSystem());
    }
}
