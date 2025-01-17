using Frontend.models;

namespace Frontend.Services;

public interface IPartnerService
{
    Partner GetPartner(string email);
}