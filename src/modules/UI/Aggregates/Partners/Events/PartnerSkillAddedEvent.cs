using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Events;

public record PartnerSkillAddedEvent(string skill): IEvent {}

public class PartnerSillAddedEventHandler : IEventHandler<Partner, PartnerSkillAddedEvent>
{
  public Partner Apply(IEventHandlerContext<Partner> context, PartnerSkillAddedEvent @event)
  {
    if (!context.Aggregate.skills.Contains(@event.skill))
    context.Aggregate
      .skills
      .Add(@event.skill);
    return context.Aggregate;
  }
}
