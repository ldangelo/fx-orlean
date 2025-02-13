using EventServer.Aggregates.Partners;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Serilog;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.FluentValidation;
using EventServer.Aggregates.Users;
using Wolverine.Http.FluentValidation;


namespace EventServer;

public class Program
{
    [Obsolete]
    private static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile("appsettings.Development.json", true, true)
            .AddEnvironmentVariables()
            .Build();
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        //
        // add wolverine/marten
        builder
            .Services.AddMarten(opts =>
            {
                opts.Connection(builder.Configuration.GetConnectionString("EventStore")!);
                opts.AutoCreateSchemaObjects = AutoCreate.All;
                opts.UseNewtonsoftForSerialization();


                opts.Events.StreamIdentity = StreamIdentity.AsString;
                opts.Events.AppendMode = EventAppendMode.Quick;

                opts.Projections.UseIdentityMapForAggregates = true;

                opts.Projections.Add<PartnerProjection>(ProjectionLifecycle.Inline);
                opts.Projections.Add<UserProjection>(ProjectionLifecycle.Inline);
            })
            .OptimizeArtifactWorkflow()
            .UseLightweightSessions()
            .IntegrateWithWolverine() // forward martin events too wolverine outbox
            .EventForwardingToWolverine();
//            .AddAsyncDaemon(DaemonMode.HotCold);


        builder.Host.UseWolverine(opts =>
        {
            opts.UseFluentValidation();
            opts.Policies.AutoApplyTransactions();
            opts.OptimizeArtifactWorkflow();
        }).StartAsync();

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
            store.Advanced.Clean.DeleteAllDocuments();
            store.Advanced.Clean.DeleteAllEventData();
        }

        app.UseHttpsRedirection();

        app.MapControllers();
        app.MapWolverineEndpoints(opts => {
            opts.ConfigureEndpoints(httpChain => {
//                httpChain.WithMetadata(new CustomMetadata());
                });
            opts.UseFluentValidationProblemDetailMiddleware();
            });


        app.Run();
    }
}
