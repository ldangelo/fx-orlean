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
    }

    public void ConfigureServices(HostBuilderContext builder, IServiceCollection services)
    {
    }
}
