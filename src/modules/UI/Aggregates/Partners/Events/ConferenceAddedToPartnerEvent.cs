using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Events;

public record ConferenceAddedToPartnerEvent(string conferenceId): IEvent {};

public class ConferenceAddedToPartnerEventHandler: IEventHandler<PartnerAggregate, ConferenceAddedToPartnerEvent>
{
  public PartnerAggregate Apply(IEventHandlerContext<PartnerAggregate> context, ConferenceAddedToPartnerEvent @event)
  {
    if (!context.Aggregate.videoConferences.Contains(@event.conferenceId))
    {
      context.Aggregate.videoConferences.Add(@event.conferenceId);
    }
    return context.Aggregate;
  }
}