using EventServer.Aggregates;
using EventServer.Aggregates.Partners.Events;
using EventServer.Aggregates.VideoConference.Events;
using org.fortium.fx.Aggregates;
using Orleankka;
using Orleans.Streams;

namespace EventServer.Aggregates;

public interface IPartnerDispatcher : IActorGrain, IGrainWithStringKey { }

[ImplicitStreamSubscription("")]
public class PartnerDispatcher : DispatchActorGrain, IPartnerDispatcher, IGrainWithStringKey
{
    private async Task On(Activate _)
    {
        var streamProvider = this.GetStreamProvider("partner");
        var key = this.GetPrimaryKey();
        var stream = streamProvider.GetStream<IEventEnvelope>(StreamId.Create("Partner", key));

        await stream.SubscribeAsync((envelope, _) => Receive(envelope));
    }

    private async Task On(EventEnvelope<VideoConferenceCreatedEvent> e)
    {
        await System
            .ActorOf<IPartnerAggregate>(e.Event.PartnerId)
            .Tell(new VideoConferenceAddedToPartnerEvent(Id, e.Event.ConferenceId));
    }
}

