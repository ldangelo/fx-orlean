using System.Diagnostics.Metrics;
using System.Reflection;
using FastEndpoints;
using FastEndpoints.Swagger;
using Keycloak.AuthServices.Authentication;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleankka.Cluster;
using Weasel.Core;

namespace common;

public static class FxHostingExtension
{
    public static IHostApplicationBuilder UseFx(this WebApplicationBuilder builder, Assembly? assembly = null)
    {
        //    builder.Services.AddKeycloak(builder, assembly);
        builder.Services.AddOrleankka(builder, assembly);
        builder.Services.AddEventStore(builder, assembly);
        builder.Services.AddOpenTelemetryFx(builder, assembly);
        builder.Services.AddFastEndpointsFx(builder, assembly);
        return builder;
    }

    internal static IServiceCollection AddEventStore(this IServiceCollection services, WebApplicationBuilder builder,
        Assembly? assembly = null)
    {
        var connString = Environment.GetEnvironmentVariable("EVENTSTORE_CONNECTION_STRING") ??
                         "Host=localhost;Port=2113;DefaultDatabase=eventstore";
        services.AddMarten(options =>
        {
            options.Connection(connString);
            options.UseNewtonsoftForSerialization();

            if (builder.Environment.IsDevelopment()) options.AutoCreateSchemaObjects = AutoCreate.All;
        }).OptimizeArtifactWorkflow();
        return services;
    }

    internal static IServiceCollection AddKeycloak(this IServiceCollection services, WebApplicationBuilder builder,
        Assembly? assembly = null)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddKeycloakWebAppAuthentication(builder.Configuration);
        services.AddKeycloakWebApiAuthentication(builder.Configuration);
        return services;
    }

    internal static IServiceCollection AddOrleankka(this IServiceCollection services, WebApplicationBuilder builder,
        Assembly? assembly)
    {
        builder.Host.UseOrleankka()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering()
                    .AddMemoryStreams("sms")
                    .AddMemoryGrainStorage("sms")
                    .AddMemoryStreams("conferences")
                    .AddMemoryGrainStorage("conferences")
                    .AddMemoryGrainStorage("partner")
                    .AddMemoryGrainStorage("users");
                /*
                siloBuilder.AddAdoNetGrainStorage("sms", options =>
                {
                    // Configure AdoNetGrainStorageOptions if needed
                    options.Invariant = "Npgsql"; // Postgresql
                    options.ConnectionString = builder.Configuration.GetConnectionString("OrleansStorage");
                });
                */
            });

        return services;
    }

    internal static IServiceCollection AddFastEndpointsFx(this IServiceCollection services,
        IHostApplicationBuilder builder, Assembly? assembly)
    {
        services.AddFastEndpoints()
            .SwaggerDocument();
        return services;
    }

    internal static IServiceCollection AddOpenTelemetryFx(this IServiceCollection services,
        IHostApplicationBuilder appBuilder, Assembly? assembly)
    {
        // Configure OpenTelemetry logging, metrics, & tracing with auto-start using the
        // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
        // Note: Switch between Zipkin/OTLP/Console by setting UseTracingExporter in appsettings.json.
        var tracingExporter = appBuilder.Configuration.GetValue("UseTracingExporter", "console")!.ToLowerInvariant();

        // Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
        var metricsExporter = appBuilder.Configuration.GetValue("UseMetricsExporter", "console")!.ToLowerInvariant();

        // Note: Switch between Console/OTLP by setting UseLogExporter in appsettings.json.
        var logExporter = appBuilder.Configuration.GetValue("UseLogExporter", "console")!.ToLowerInvariant();

        // Note: Switch between Explicit/Exponential by setting HistogramAggregation in appsettings.json
        var histogramAggregation =
            appBuilder.Configuration.GetValue("HistogramAggregation", "explicit")!.ToLowerInvariant();

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    appBuilder.Configuration.GetValue("ServiceName", "otel-test")!,
                    serviceVersion: assembly?.GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(builder =>
            {
// Tracing

// Ensure the TracerProvider subscribes to any custom ActivitySources.
                builder
                    .AddSource("FX").SetSampler(new AlwaysOnSampler()).AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                // Use IConfiguration binding for AspNetCore instrumentation options.
                services.Configure<AspNetCoreTraceInstrumentationOptions>(
                    appBuilder.Configuration.GetSection("AspNetCoreInstrumentation"));
                Console.WriteLine("Tracing exporter " + tracingExporter);
                switch (tracingExporter)
                {
                    case "jaeger":
                        builder.AddJaegerExporter();

                        builder.ConfigureServices(services =>
                        {
                            services.Configure<JaegerExporterOptions>(
                                appBuilder.Configuration.GetSection("Jaeger"));
                        });
                        break;
                    case "zipkin":
                        builder.AddZipkinExporter();

                        builder.ConfigureServices(services =>
                        {
                            // Use IConfiguration binding for Zipkin exporter options.
                            services.Configure<ZipkinExporterOptions>(appBuilder.Configuration.GetSection("Zipkin"));
                        });
                        break;

                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint =
                                new Uri(appBuilder.Configuration.GetValue("Otlp:Endpoint", "http://localhost:4317")!);
                        });
                        break;

                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            })
            .WithMetrics(builder =>
            {
                // Metrics

                // Ensure the MeterProvider subscribes to any custom Meters.
//            .AddMeter(OpenTelemetry.Instrumentation)
                builder
                    .SetExemplarFilter(ExemplarFilterType.TraceBased)
//            .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (histogramAggregation)
                {
                    case "exponential":
                        builder.AddView(instrument =>
                        {
                            return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                                ? new Base2ExponentialBucketHistogramConfiguration()
                                : null;
                        });
                        break;
                }

                switch (metricsExporter)
                {
                    case "prometheus":
                        builder.AddPrometheusExporter();
                        break;
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint =
                                new Uri(appBuilder.Configuration.GetValue("Otlp:Endpoint", "http://localhost:4317")!);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            })
            .WithLogging(builder =>
            {
                // Note: See appsettings.json Logging:OpenTelemetry section for configuration.
                Console.WriteLine("Logging exporter " + logExporter);
                switch (logExporter)
                {
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint =
                                new Uri(appBuilder.Configuration.GetValue("Otlp:Endpoint", "http://localhost:4317")!);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            });

        return services;
    }
}
