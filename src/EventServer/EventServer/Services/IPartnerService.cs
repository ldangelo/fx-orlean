using org.fortium.fx.common;

namespace EventServer.Client.Services;

public interface IPartnerService
{
    //
    // get the partnersnapshot of the currently authenticated partner
    Task<Partner> GetPartner(string email);
}