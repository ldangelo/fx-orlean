using EventServer.Aggregates.Partners;
using org.fortium.fx.common;

namespace EventServer.Services;

public interface IPartnerService
{
    //
    // get the partnersnapshot of the currently authenticated partner
    Task<Partner?> GetPartner(string email);
}

