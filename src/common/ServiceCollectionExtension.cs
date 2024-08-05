using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace org.fortium.fx;

public static class ServiceCollectionExtensions
{
    public static IHostBuilder AddOrleans(this IHostBuilder host)
    {
        host.UseOrleans(static siloBuilder =>
        {
            siloBuilder.UseLocalhostClustering();
            siloBuilder.AddMemoryGrainStorage("fx");
        });

        return host;
    }

}
