using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Hosting;
using Serilog;
using Xunit.Abstractions;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace UI.Tests;

public class Startup
{
    private IHost _host;

    public async void ConfigureHost(IHostBuilder builder)
    {
        _host = new HostBuilder()
            .UseOrleans(c => c.UseLocalhostClustering())
            .UseOrleankka()
            .Build();

        await _host.StartAsync();
    }

    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton<IActorSystem>(_host.ActorSystem());
    }
}
