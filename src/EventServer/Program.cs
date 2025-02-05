using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Serilog;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

public class Program
{
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
        // add wolverine/marten
        builder.Services.AddWolverine(opts =>
        {
            opts.ApplicationAssembly = typeof(Program).Assembly;
            opts.Policies.AutoApplyTransactions();
        });
        builder
            .Services.AddMarten(opts =>
            {
                opts.Connection(builder.Configuration.GetConnectionString("EventStore")!);
                opts.AutoCreateSchemaObjects = AutoCreate.All;
                opts.UseNewtonsoftForSerialization();
                opts.Events.StreamIdentity = StreamIdentity.AsString;

                //                opts.Projections.Add<PartnerProjection>(ProjectionLifecycle.Async);
            })
            .UseLightweightSessions()
            .IntegrateWithWolverine()
            .AddAsyncDaemon(DaemonMode.HotCold);

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
        }

        app.UseHttpsRedirection();

        app.MapControllers();
        app.MapWolverineEndpoints();

        app.Run();
    }
}

