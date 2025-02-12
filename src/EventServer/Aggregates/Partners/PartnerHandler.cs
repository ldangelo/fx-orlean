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
    }

    public static void Handle(PartnerLoggedOutEvent evnt, Partner p)
    {
        Log.Information("PartnerHandler: Applying logout event to {EmailAddress}", evnt.EmailAddress);
        p.LastLogout = evnt.LogoutTime;
        p.LoggedIn = false;
    }

    public static void Handle(PartnerCreatedEvent evnt, Partner p)
    {
        Log.Information("PartnerHandler: Creating partner {EmailAddress}", evnt.EmailAddress);
        p.FirstName = evnt.FirstName;
        p.LastName = evnt.LastName;
        p.EmailAddress = evnt.EmailAddress;
        p.LoggedIn = false;
    }
}
