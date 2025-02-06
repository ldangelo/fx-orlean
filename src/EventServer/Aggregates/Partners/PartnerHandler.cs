using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Partners.Events;

using Serilog;

namespace EventServer.Aggregates.Partners;


public class PartnerHandler {
        public static void Handle(PartnerLoggedInEvent evnt, Partner p)
    {
        Log.Information("Partner: Applying login event to {EmailAddress}", evnt.partnerId);
        p.LastLogin = evnt.loginTime;
        p.LoggedIn = true;
    }

    public static void Handle(PartnerLoggedOutEvent evnt, Partner p)
    {
        Log.Information("Partner: Applying logout event to {EmailAddress}", evnt.partnerId);
        p.LastLogin = evnt.logoutTime;
        p.LoggedIn = false;
    }

    public static void Handle(PartnerCreatedEvent evnt, Partner p)
        {
        Log.Information("Creating partner {EmailAddress}", evnt.partnerId);
            p.FirstName = evnt.firstName;
            p.LastName = evnt.lastName;
            p.EmailAddress = evnt.emailAddress;
            p.LoggedIn = false;
        }


}
