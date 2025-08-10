using Alba;
using Fortium.Types;
using JasperFx.CommandLine;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Serilog;
using Serilog.Events;
using Wolverine;
using Wolverine.Runtime;
using Wolverine.Tracking;
using Xunit.Abstractions;

namespace EventServer.Tests;

#region sample_AppFixture_in_incident_service_testing

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost? Host { get; private set; }

    public async Task InitializeAsync()
    {
        JasperFxEnvironment.AutoStartHost = true;

        // This is bootstrapping the actual application using
        // its implied Program.Main() set up
        Host = await AlbaHost.For<Program>(x =>
        {
            // Just showing that you *can* override service
            // registrations for testing if that's useful
            x.ConfigureServices((context, services) =>
                {
                    // If wolverine were using Rabbit MQ / SQS / Azure Service Bus,
                    // turn that off for now
                    services.DisableAllExternalWolverineTransports();
                }
            );
        });
    }

    public async Task DisposeAsync()
    {
        if (Host != null)
        {
            await Host.StopAsync();
            Thread.Sleep(3000);
            Host.Dispose();
        }
    }
}

#endregion

[CollectionDefinition("integration")]
public class IntegrationCollection : ICollectionFixture<AppFixture>;

[Collection("integration")]
public abstract class IntegrationContext : IAsyncLifetime
{
    private readonly AppFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected IntegrationContext(AppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        Runtime = _fixture.Host?.Services.GetRequiredService<IWolverineRuntime>() as WolverineRuntime;
    }

    public WolverineRuntime? Runtime { get; }
    
    protected ITestOutputHelper Output => _output;

    public IAlbaHost? Host => _fixture.Host;
    public IDocumentStore? Store => _fixture.Host?.Services.GetRequiredService<IDocumentStore>();

    async Task IAsyncLifetime.InitializeAsync()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.TestOutput(_output, LogEventLevel.Debug)
            .CreateLogger();

        if (Store != null)
        {
            // Using Marten, wipe out all data and reset the state
            // back to exactly what we described in InitialAccountData
            await Store.Advanced.ResetAllData();

            //
            //  Let's create a couple of partners to test against
            var leo = new Partner();

            leo.FirstName = "Leo";
            leo.LastName = "DAngelo";
            leo.EmailAddress = "leo.dangelo@fortiumpartners.com";
            leo.Skills.Add(new PartnerSkill("leadership", 30, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("architecture", 30, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("aws", 30, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("agile", 20, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("test driven development", 20, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("AI", 20, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("dotnet", 20, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("c#", 20, ExperienceLevel.Expert));
            leo.Skills.Add(new PartnerSkill("java", 30, ExperienceLevel.Expert));
            leo.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), null,
                "Fortium Partners", "CTO",
                "Fractional CTO with experience working with SaaS companies in the financial services industry"));
            leo.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-6)),
                DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
                "Allied Payment Network", "CTO",
                "As CTO, I was responsible for the development of the company's payments platform. I led a team of 15 C# developers, 7 QA engineers, and 2 business analysts. To produce a state of the art consumer bill payment platform, I was responsible for the development of a new payment platform that was designed to be scalable, flexible, and easy to use."));
            leo.AvailabilityNext30Days = 160;

            var burke = new Partner();
            burke.FirstName = "Burke";
            burke.LastName = "Autrey";
            burke.EmailAddress = "burke.autrey@fortiumpartners.com";
            burke.Skills.Add(new PartnerSkill("leadership", 30, ExperienceLevel.Expert));
            burke.Skills.Add(new PartnerSkill("strategic thinking", 30, ExperienceLevel.Expert));
            burke.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), null,
                "Fortium Partners", "CEO", "Founder and CEO of Fortium Partners"));
            burke.AvailabilityNext30Days = 160;

            var session = Store.LightweightSession();
            session.Store(leo);
            session.Store(burke);
        }
    }

    // This is required because of the IAsyncLifetime
    // interface. Note that I do *not* tear down database
    // state after the test. That's purposeful
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public Task<IScenarioResult> Scenario(Action<Scenario> configure)
    {
        if (Host == null)
        {
            throw new InvalidOperationException("Host is not initialized");
        }
        return Host.Scenario(configure);
    }

    // This method allows us to make HTTP calls into our system
    // in memory with Alba, but do so within Wolverine's test support
    // for message tracking to both record outgoing messages and to ensure
    // that any cascaded work spawned by the initial command is completed
    // before passing control back to the calling test
    protected async Task<(ITrackedSession, IScenarioResult)> TrackedHttpCall(
        Action<Scenario> configuration
    )
    {
        IScenarioResult result = null!;

        // The outer part is tying into Wolverine's test support
        // to "wait" for all detected message activity to complete
        var tracked = await Host!.ExecuteAndWaitAsync(async () =>
        {
            // The inner part here is actually making an HTTP request
            // to the system under test with Alba
            result = await Host.Scenario(configuration);
        });

        return (tracked, result);
    }
}
