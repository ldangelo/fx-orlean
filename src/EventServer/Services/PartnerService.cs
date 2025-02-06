using System.Security.Claims;
using EventServer.Aggregates.Partners;

namespace EventServer.Services;

public class PartnerService : IPartnerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PartnerService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Partner?>? GetPartner(string email)
    {
        return null;
    }

    private ClaimsPrincipal? GetUserDetails()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}
