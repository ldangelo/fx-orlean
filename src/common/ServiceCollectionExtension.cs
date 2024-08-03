using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Builder;

namespace org.fortium.fx;

public static class ServiceCollectionExtensions
{
    public static void AddFx(this WebApplicationBuilder builder, string assemblyName = "")
    {
//       builder.Services.SwaggerDocument(o => {
//           o.DocumentSettings = s =>
//           {
//               s.Title = "FX Api";
//               s.Version = "v1";
//           };
//       });


    }

  public static IServiceCollection AddFx(this IServiceCollection services, string assembly = "")
  {
    if (!string.IsNullOrWhiteSpace(assembly))
    {
      Assembly.Load(assembly);
    }

    services.AddOpenTelemetryFx();
    services.AddFastEndpointsFx();
    return services;
  }


  public static IServiceCollection AddFastEndpointsFx(this IServiceCollection services) {
 //  services.AddFastEndpoints()
 //       .SwaggerDocument();
    return services;
  }

  public static IServiceCollection AddOpenTelemetryFx(this IServiceCollection services) {
           services.AddOpenTelemetry()
           .ConfigureResource(resource => resource.AddService("FX"))
           .WithTracing(tracing =>
           {
               tracing.AddHttpClientInstrumentation();
               tracing.AddHttpClientInstrumentation();
               tracing.AddConsoleExporter();
               tracing.AddOtlpExporter();
               tracing.AddSource("Microsoft.Orleans.Runtime");
               tracing.AddSource("Microsoft.Orleans.Application");
           })
           .WithMetrics(metrics =>
           {
               metrics.AddHttpClientInstrumentation();
               metrics.AddAspNetCoreInstrumentation();
               metrics.AddConsoleExporter();
               metrics.AddOtlpExporter();
               metrics.AddMeter("Microsoft.Orleans");
           });


       return services;
    }

    public static ILoggingBuilder AddOpenTelemetryLoggingFx(this ILoggingBuilder builder)
    {

        builder.AddOpenTelemetry(options => {
            options.AddConsoleExporter();
            options.AddOtlpExporter();
        });
        return builder;
    }

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
