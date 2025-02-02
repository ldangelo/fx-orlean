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

    public async Task<Partner> GetPartner(string email)
    {
        var actorRef = actorSystem.ActorOf<IPartnerAggregate>(email);

        var partner = await actorRef.Ask<Partner>(new GetPartnerDetails());

        return partner;
    }
}