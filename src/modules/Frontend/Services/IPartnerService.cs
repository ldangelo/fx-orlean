using org.fortium.fx.common;

namespace Frontend.Services;

public interface IPartnerService
{
    Task<Partner> GetPartner(string email);
}