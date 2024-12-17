using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Events;

public record PartnerSkillAddedEvent(string skill): IEvent {}

public class PartnerSillAddedEventHandler : IEventHandler<PartnerAggregate, PartnerSkillAddedEvent>
{
  public PartnerAggregate Apply(IEventHandlerContext<PartnerAggregate> context, PartnerSkillAddedEvent @event)
  {
    if (!context.Aggregate.skills.Contains(@event.skill))
    context.Aggregate
      .skills
      .Add(@event.skill);
    return context.Aggregate;
  }
}
