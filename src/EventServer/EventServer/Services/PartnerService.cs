using System.Security.Claims;
using common.Queries;
using org.fortium.fx.Aggregates;
using org.fortium.fx.common;
using Orleankka;
using Orleankka.Client;

namespace EventServer.Client.Services;

public class PartnerService : IPartnerService
{
    private readonly IClientActorSystem _actorSystem;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PartnerService(IClientActorSystem actorSystem, IHttpContextAccessor httpContextAccessor)
    {
        _actorSystem = actorSystem;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PartnerSnapshot?> GetPartner(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var actorRef = _actorSystem.ActorOf<IPartnerAggregate>(email);

            var partnerSnapshot = await actorRef.Ask<PartnerSnapshot>(new GetPartnerDetails());

            return partnerSnapshot;
        }

        return null;
    }

    private ClaimsPrincipal? GetUserDetails()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}