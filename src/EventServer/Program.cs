using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Payments;
using EventServer.Aggregates.Users;
using EventServer.Aggregates.VideoConference;
using FastEndpoints;
using FastEndpoints.Swagger;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Serilog;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Marten;
using Fortium.Types;
using JasperFx;

namespace EventServer;

public class Program
{
    [Obsolete]
    private static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile("appsettings.Development.json", true, true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();
        builder.Services.AddFastEndpoints().SwaggerDocument();

        builder.Services.AddControllers();
        builder.Environment.ApplicationName = "EventServer";

//        if (builder.Environment.IsDevelopment())
//            builder.Services.AddHostedService<StartUpTask>();
        //
        // add wolverine/marten
        builder
            .Services.AddMarten(opts =>
            {
                opts.Connection(builder.Configuration.GetConnectionString("EventStore")!);
                opts.UseNewtonsoftForSerialization();

                if (builder.Environment.IsDevelopment())
                    opts.AutoCreateSchemaObjects = AutoCreate.All;

                opts.Events.StreamIdentity = StreamIdentity.AsString;
                opts.Events.AppendMode = EventAppendMode.Rich;

//                opts.Schema.For<Partner>().UseOptimisticConcurrency(true);

                opts.Projections.Add<PartnerProjection>(ProjectionLifecycle.Inline);
                opts.Projections.Add<UserProjection>(ProjectionLifecycle.Inline);
                opts.Projections.Add<PaymentProjection>(ProjectionLifecycle.Inline);
                opts.Projections.Add<VideoConferenceProjection>(ProjectionLifecycle.Inline);
            })
//            .OptimizeArtifactWorkflow()
            .UseLightweightSessions()
            .IntegrateWithWolverine() // forward martin events too wolverine outbox
            .EventForwardingToWolverine();
//            .AddAsyncDaemon(DaemonMode.HotCold);


        builder
            .Host.UseWolverine(opts =>
            {
                opts.UseFluentValidation();
                opts.Policies.AutoApplyTransactions();
//                opts.OptimizeArtifactWorkflow();
            });

        builder.Services.AddSingleton<ChatGPTWithRAG>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddWolverineHttp();

        var app = builder.Build();
        //
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();

            var store = app.Services.GetRequiredService<IDocumentStore>();
            await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(Partner));  // Changed from PartnerProjection to Partner
            await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(User));    // Assuming User is the document type
            await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(Payment)); // Assuming Payment is the document type
            await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(VideoConferenceState));
        }

        app.UseFastEndpoints().UseSwaggerGen();
        app.UseHttpsRedirection();

        app.MapControllers();
        app.MapWolverineEndpoints(opts =>
        {
            opts.ConfigureEndpoints(httpChain =>
            {
                //                httpChain.WithMetadata(new CustomMetadata());
            });
            opts.UseFluentValidationProblemDetailMiddleware();
        });
        
        // Add custom exception handler for KeyNotFoundException
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (exceptionHandlerFeature?.Error is KeyNotFoundException)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync(exceptionHandlerFeature.Error.Message);
                }
                else
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An error occurred");
                }
            });
        });

        // Define the minimal API endpoint

        app.Run();
    }
}
