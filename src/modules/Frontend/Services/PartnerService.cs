using Frontend.models;

namespace Frontend.Services;

public class PartnerService: IPartnerService
{
    public Partner GetPartner(string email)
    {
        return new Partner(null, null, email, null);    
    }
}