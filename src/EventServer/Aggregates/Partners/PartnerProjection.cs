using EventServer.Aggregates.Partners.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using org.fortium.fx.common;
using Serilog;
using Wolverine.Attributes;

namespace EventServer.Aggregates.Partners;

public class PartnerProjection : SingleStreamProjection<Partner>
{
    public static Partner Apply(PartnerCreatedEvent @event, Partner partner) {
        Log.Information("PartnerProjection: Applying {type} to {EmailAddress}", typeof(PartnerCreatedEvent),@event.partnerId);

        partner.FirstName = @event.firstName;
        partner.LastName = @event.lastName;
        partner.EmailAddress = @event.emailAddress;
        partner.CreateDate = DateTime.Now;
        return partner;
    }
    public static Partner Apply(PartnerLoggedInEvent loggedInEvent, Partner partner)
    {
        Log.Information("PartnerProjection: Applying login event to {EmailAddress}", loggedInEvent.partnerId);
        partner.Active = true;
        partner.LoggedIn = true;
        partner.UpdateDate = DateTime.Now;
        partner.LastLogin = loggedInEvent.loginTime;
        return partner;
    }

    public static Partner Apply(PartnerLoggedOutEvent @event, Partner partner)
    {
        Log.Information("PartnerProjection: Applying login event to {EmailAddress}", @event.partnerId);
        partner.LoggedIn = false;
        partner.LastLogin = @event.logoutTime;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }
}
