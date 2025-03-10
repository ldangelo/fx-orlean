using Fortium.Types;
using Marten;
using Serilog;
using Wolverine.Persistence;

namespace EventServer;

public class StartUpTask : IHostedService
{
    private readonly IDocumentStore _store;

    public StartUpTask(IDocumentStore store)
    {
        this._store = store;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting up");

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

        _store.BulkInsert<Partner>([leo, burke]);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}