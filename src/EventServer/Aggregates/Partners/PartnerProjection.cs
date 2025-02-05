using EventServer.Aggregates.Partners.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using org.fortium.fx.common;
using Serilog;
using Wolverine.Attributes;

namespace EventServer.Aggregates.Partners;

public class PartnerProjection : SingleStreamProjection<Partner>
{
    public static Partner Apply(PartnerLoggedInEvent loggedInEvent, Partner partner)
    {
        Log.Information("Applying login event to {EmailAddress}", loggedInEvent.partnerId);
        partner.Active = true;
        partner.LastLogin = loggedInEvent.loginTime;
        return partner;
    }
}
