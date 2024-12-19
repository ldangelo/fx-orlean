using Serilog;
using Serilog.Context;
using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Events;

public record ConferenceAddedToPartnerEvent(string conferenceId) : IEvent
{
};

public class ConferenceAddedToPartnerEventHandler : IEventHandler<PartnerAggregate, ConferenceAddedToPartnerEvent>
{
    public PartnerAggregate Apply(IEventHandlerContext<PartnerAggregate> context, ConferenceAddedToPartnerEvent @event)
    {
        if (!context.Aggregate.videoConferences.Contains(@event.conferenceId))
        {
            LogContext.PushProperty("ConferenceId", @event.conferenceId);
            LogContext.PushProperty("PartnerId", context.Aggregate.emailAddress);
            Log.Information("Add video conference to partner");
            context.Aggregate.videoConferences.Add(@event.conferenceId);
        }

        return context.Aggregate;
    }
}