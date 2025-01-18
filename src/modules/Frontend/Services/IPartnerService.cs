using org.fortium.fx.common;

namespace Frontend.Services;

public interface IPartnerService
{
    Task<PartnerSnapshot> GetPartner(string email);
}