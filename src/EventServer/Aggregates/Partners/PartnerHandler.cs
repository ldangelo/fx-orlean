using EventServer.Aggregates.Partners.Events;
using Serilog;
using Fortium.Types;


namespace EventServer.Aggregates.Partners;

public class PartnerHandler
{
    public static void Handle(PartnerLoggedInEvent evnt, Partner p)
    {
        Log.Information("PartnerHandler: Applying login event to {EmailAddress}", evnt.EmailAddress);
        p.Active = true;
        p.LastLogin = evnt.LoginTime;
        p.LoggedIn = true;
        p.UpdateDate = DateTime.Now;
    }

    public static void Handle(PartnerLoggedOutEvent evnt, Partner p)
    {
        Log.Information("PartnerHandler: Applying logout event to {EmailAddress}", evnt.EmailAddress);
        p.LastLogout = evnt.LogoutTime;
        p.LoggedIn = false;
        p.UpdateDate = DateTime.Now;
    }

    public static void Handle(PartnerCreatedEvent evnt, Partner p)
    {
        Log.Information("PartnerHandler: Creating partner {EmailAddress}", evnt.EmailAddress);
        p.FirstName = evnt.FirstName;
        p.LastName = evnt.LastName;
        p.EmailAddress = evnt.EmailAddress;
        p.LoggedIn = false;
        p.CreateDate = DateTime.Now;
    }

    public static void Handle(PartnerSkillAddedEvent @event, Partner p) {
        Log.Information("PartnerHandler: Adding skill to {EmailAddress}", @event.EmailAddress);
        p.Skills.AddRange(@event.Skills);
        p.UpdateDate = DateTime.Now;
    }

    public static void Handle(PartnerBioUpdatedEvent @event, Partner p) {
        Log.Information("PartnerHandler: Updating bio for {EmailAddress}", @event.EmailAddress);
        p.Bio = @event.Bio;
        p.UpdateDate = DateTime.Now;
    }

    public static void Handle(SetPartnerPhotoUrlEvent @event, Partner p) {
        Log.Information("PartnerHandler: Applying photo url updated event to {EmailAddress}", @event.EmailAddress);
        p.PhotoUrl = @event.PhotoUrl;
        p.UpdateDate = DateTime.Now;
    }

    public static void Handle(SetPartnerPrimaryPhoneEvent @event, Partner p) {
        Log.Information("PartnerHandler: Applying primary phone updated event to {EmailAddress}, {phone}", @event.EmailAddress, @event.PrimaryPhone);
        p.PrimaryPhone = @event.PrimaryPhone;
        p.UpdateDate = DateTime.Now;
    }
}
