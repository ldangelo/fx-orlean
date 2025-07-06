using EventServer.Aggregates.Partners.Events;
using Fortium.Types;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using Marten.Schema.Identity;
using Serilog;
using Wolverine.Attributes;

namespace EventServer.Aggregates.Partners;

public class PartnerIdGeneration : IIdGeneration
{
    public IEnumerable<Type> KeyTypes => new Type[] { typeof(string) };

    public bool RequiresSequences => false;

    public void GenerateCode(GeneratedMethod method, DocumentMapping mapping)
    {
        var document = new Use(mapping.DocumentType);

        method.Frames.Code($"_setter({{0}}, \"newId\");", document);
        method.Frames.Code($"return {{0}}.{mapping.CodeGen?.AccessId};", document);
    }
}

public class PartnerProjection : SingleStreamProjection<Partner>
{
    public static Partner Apply(PartnerCreatedEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying {type} to {EmailAddress}",
            typeof(PartnerCreatedEvent),
            @event.EmailAddress
        );

        partner.FirstName = @event.FirstName;
        partner.LastName = @event.LastName;
        partner.EmailAddress = @event.EmailAddress;
        partner.CreateDate = DateTime.Now;
        return partner;
    }

    public static Partner Apply(PartnerLoggedInEvent loggedInEvent, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying login event to {EmailAddress}",
            loggedInEvent.EmailAddress
        );
        partner.Active = true;
        partner.LoggedIn = true;
        partner.UpdateDate = DateTime.Now;
        partner.LastLogin = loggedInEvent.LoginTime;
        return partner;
    }

    public static Partner Apply(PartnerLoggedOutEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying login event to {EmailAddress}",
            @event.EmailAddress
        );
        partner.LoggedIn = false;
        partner.LastLogout = @event.LogoutTime;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }

    public static Partner Apply(PartnerSkillAddedEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying skill added event to {EmailAddress}",
            @event.EmailAddress
        );
        partner.Skills.AddRange(@event.Skills);
        partner.UpdateDate = DateTime.Now;

        return partner;
    }

    public static Partner Apply(PartnerBioUpdatedEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying bio updated event to {EmailAddress}",
            @event.EmailAddress
        );
        partner.Bio = @event.Bio;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }

    public static Partner Apply(SetPartnerPrimaryPhoneEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying primary phone updated event to {EmailAddress}",
            @event.EmailAddress
        );
        partner.PrimaryPhone = @event.PrimaryPhone;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }

    public static Partner Apply(SetPartnerPhotoUrlEvent @event, Partner partner)
    {
        Log.Information(
            "PartnerProjection: Applying photo url updated event to {EmailAddress}",
            @event.EmailAddress
        );
        partner.PhotoUrl = @event.PhotoUrl;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }
}
