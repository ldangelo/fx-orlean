using common.Queries;
using org.fortium.fx.Aggregates;
using org.fortium.fx.common;
using Orleankka;
using Orleankka.Client;

namespace Frontend.Services;

public class PartnerService : IPartnerService
{
    private readonly IClientActorSystem actorSystem;

    public PartnerService(IClientActorSystem actorSystem)
    {
        this.actorSystem = actorSystem;
    }

    public async Task<PartnerSnapshot> GetPartner(string email)
    {
        var actorRef = actorSystem.ActorOf<IPartnerAggregate>(email);

        var partnerSnapshot = await actorRef.Ask<PartnerSnapshot>(new GetPartnerDetails());

        return partnerSnapshot;
    }
}