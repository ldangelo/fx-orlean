using EventServer.Aggregates.Partners.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Serilog;
using Wolverine.Attributes;
using Fortium.Types;


namespace EventServer.Aggregates.Partners;

public class PartnerProjection : SingleStreamProjection<Partner>
{
    public static Partner Apply(PartnerCreatedEvent @event, Partner partner) {
        Log.Information("PartnerProjection: Applying {type} to {EmailAddress}", typeof(PartnerCreatedEvent),@event.EmailAddress);

        partner.FirstName = @event.FirstName;
        partner.LastName = @event.LastName;
        partner.EmailAddress = @event.EmailAddress;
        partner.CreateDate = DateTime.Now;
        return partner;
    }
    public static Partner Apply(PartnerLoggedInEvent loggedInEvent, Partner partner)
    {
        Log.Information("PartnerProjection: Applying login event to {EmailAddress}", loggedInEvent.EmailAddress);
        partner.Active = true;
        partner.LoggedIn = true;
        partner.UpdateDate = DateTime.Now;
        partner.LastLogin = loggedInEvent.LoginTime;
        return partner;
    }

    public static Partner Apply(PartnerLoggedOutEvent @event, Partner partner)
    {
        Log.Information("PartnerProjection: Applying login event to {EmailAddress}", @event.EmailAddress);
        partner.LoggedIn = false;
        partner.LastLogout = @event.LogoutTime;
        partner.UpdateDate = DateTime.Now;

        return partner;
    }
}
