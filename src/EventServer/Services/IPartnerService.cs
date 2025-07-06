using EventServer.Aggregates.Partners;
using Fortium.Types;


namespace EventServer.Services;

public interface IPartnerService
{
    //
    // get the partnersnapshot of the currently authenticated partner
    Partner GetPartner(string email);
}

