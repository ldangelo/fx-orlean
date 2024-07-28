using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace org.fortium.fx;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTelemetryFx(this IServiceCollection services,
                                                        string assembly = "") {
       if (!string.IsNullOrWhiteSpace(assembly))
           {
               Assembly.Load(assembly);
           }

       services.AddOpenTelemetry()
           .WithTracing(tracing => tracing.AddOtlpExporter())
           .WithMetrics(metrics => metrics.AddOtlpExporter());

       return services;
    }

    public static ILoggingBuilder AddOpenTelemetryLoggingFx(this ILoggingBuilder builder)
    {

        builder.AddOpenTelemetry(options => {
            options.AddOtlpExporter();
        });
        return builder;
    }
}
