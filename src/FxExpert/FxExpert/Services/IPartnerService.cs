using FxExpert.Aggregates.Partners;

namespace EventServer.Client.Services;

public interface IPartnerService
{
    //
    // get the partnersnapshot of the currently authenticated partner
    Task<PartnerAggregate?> GetPartner(string email);
}