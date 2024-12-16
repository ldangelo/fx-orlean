using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Events;

public record PartnerDetailsSetEvent(string emailAddress, string firstName, string lastName): IEvent {}

public class PartnerDetailsSetEventHandler: IEventHandler<Partner, PartnerDetailsSetEvent> {
  public Partner Apply(IEventHandlerContext<Partner> context, PartnerDetailsSetEvent @event)
  {
    context.Aggregate.emailAddress = @event.emailAddress;
    context.Aggregate.firstName = @event.firstName;
    context.Aggregate.lastName = @event.lastName;
    return context.Aggregate;
  }
}
