using Fortium.Types;
using Marten;
using Serilog;

namespace EventServer;

public class StartUpTask : IHostedService
{
    private readonly IDocumentStore _store;

    public StartUpTask(IDocumentStore store)
    {
        _store = store;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting up");

        // Ensure the schema is created before inserting data
        await _store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        
        // Wait a bit more to ensure all async operations are complete
        await Task.Delay(1000, cancellationToken);
        var leo = new Partner();

        leo.FirstName = "Leo";
        leo.LastName = "DAngelo";
        leo.EmailAddress = "leo.dangelo@fortiumpartners.com";
        leo.PrimaryPhone = "+1 (972) 979-0116";
        leo.City = "Plano";
        leo.State = "TX";
        leo.Country = "USA";

        leo.Bio =
            "Leo is a seasoned technology veteran.  He has over 40 years of experience the majority of it in SaaS solutions.";
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
            "Fractional CTO with experience working with SaaS companies.  Specializing in transitioning to or improving existing SaaS solutions. "));
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

        try
        {
            using var session = _store.LightweightSession();
            
            // Check if partners already exist to avoid duplicates
            var existingLeo = await session.Query<Partner>()
                .FirstOrDefaultAsync(p => p.EmailAddress == leo.EmailAddress, cancellationToken);
            var existingBurke = await session.Query<Partner>()
                .FirstOrDefaultAsync(p => p.EmailAddress == burke.EmailAddress, cancellationToken);
            
            if (existingLeo == null)
            {
                session.Store(leo);
                Log.Information("Storing partner: {EmailAddress}", leo.EmailAddress);
            }
            else
            {
                Log.Information("Partner {EmailAddress} already exists, skipping", leo.EmailAddress);
            }
            
            if (existingBurke == null)
            {
                session.Store(burke);
                Log.Information("Storing partner: {EmailAddress}", burke.EmailAddress);
            }
            else
            {
                Log.Information("Partner {EmailAddress} already exists, skipping", burke.EmailAddress);
            }
            
            await session.SaveChangesAsync(cancellationToken);
            Log.Information("Startup data initialization completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during startup data initialization");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
