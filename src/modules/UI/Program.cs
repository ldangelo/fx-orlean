using System.Diagnostics.Metrics;
using System.Reflection;
using FastEndpoints;
using FastEndpoints.Swagger;
using Keycloak.AuthServices.Authentication;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Users;

internal static class Program
{
    internal static IServiceCollection AddFx(this IServiceCollection services, WebApplicationBuilder builder,
        string assembly = "")
    {
        if (!string.IsNullOrWhiteSpace(assembly)) Assembly.Load(assembly);

        services.AddOpenTelemetryFx(builder);
        services.AddFastEndpointsFx(builder);
        return services;
    }


    internal static IServiceCollection AddFastEndpointsFx(this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        //  services.AddFastEndpoints()
        //       .SwaggerDocument();
        return services;
    }

    internal static IServiceCollection AddOpenTelemetryFx(this IServiceCollection services,
        WebApplicationBuilder appBuilder)
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

        appBuilder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    appBuilder.Configuration.GetValue("ServiceName", "otel-test")!,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(builder =>
            {
// Tracing

// Ensure the TracerProvider subscribes to any custom ActivitySources.
                builder
                    .AddSource("FX").SetSampler(new AlwaysOnSampler()).AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                // Use IConfiguration binding for AspNetCore instrumentation options.
                appBuilder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(
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

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //
        // clear autogenerated logging providers
        builder.Logging.ClearProviders();

        //
        // Add open-telemetry common/ServiceCollectionExtensions.cs
        builder.Services.AddFx(builder);

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

        //
        // Add Orleans
        builder.Host.UseOrleans(siloBuilder => { siloBuilder.UseLocalhostClustering(); });

        builder.Services.AddFastEndpoints(options => { options.Assemblies = [UserAssembly.Get()]; })
            .SwaggerDocument(o =>
            {
                o.AutoTagPathSegmentIndex = 2;
                o.TagCase = TagCase.TitleCase;
                o.TagStripSymbols = true; //this option is new
            });

        // Add services to the container.
        builder.Services.AddSerilog();
        builder.Services.AddRazorPages();
        builder.Services.AddKeycloakWebAppAuthentication(builder.Configuration);
        //builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //
        // Add Swagger Support
        app.UseFastEndpoints().UseSwaggerGen();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages().RequireAuthorization();
        app.Logger.LogInformation("Starting FX Application");
        app.Run();
    }
}