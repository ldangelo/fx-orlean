using System.Security.Claims;
using FxExpert.Aggregates.Partners;

namespace EventServer.Client.Services;

public class PartnerService : IPartnerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PartnerService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PartnerAggregate?> GetPartner(string email)
    {
        return null;
    }

    private ClaimsPrincipal? GetUserDetails()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}