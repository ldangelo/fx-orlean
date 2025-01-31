using common.Commands;
using Orleankka;
using UI.Aggregates.Partners;

public static class DevelopmentSeedPartner
{
    public static void Initialize(IServiceProvider serviceprovider)
    {
        var system = serviceprovider.GetRequiredService<IActorSystem>();
        var partner = system.ActorOf<PartnerAggregate>("leo.dangelo@fortiumpartners.com");
        partner.Tell(new CreatePartnerCommand("Leo", "D'Angelo", "leo.dangelo@fortiumpartners.com"));
    }
}