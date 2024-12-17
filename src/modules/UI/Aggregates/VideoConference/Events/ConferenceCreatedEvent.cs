using UI.Aggregates.VideoConference;
using Whaally.Domain.Abstractions;

namespace UI.Grains.VideoConference.Commands;

public class ConferenceCreatedEvent : IEvent
{
   public DateTime StartTime { get; set; }
   public DateTime EndTime { get; set; }
   public string UserId { get; set; } = "";
   public string PartnerId { get; set; } = "";
   public ConferenceCreatedEvent(DateTime commandStartTime, DateTime commandEndTime, string commandUserId, string commandPartnerId)
   {
       StartTime = commandStartTime;
       EndTime = commandEndTime;
       UserId = commandUserId;
       PartnerId = commandPartnerId;
   }
}

public class ConferenceCreatedEventHandler : IEventHandler<VideoConferenceAggregate, ConferenceCreatedEvent>
{
    public VideoConferenceAggregate Apply(IEventHandlerContext<VideoConferenceAggregate> context, ConferenceCreatedEvent @event)
    {
        context.Aggregate.StartTime = @event.StartTime;
        context.Aggregate.EndTime = @event.EndTime;
        context.Aggregate.UserId = @event.UserId;
        context.Aggregate.PartnerId = @event.PartnerId;
        
        return new VideoConferenceAggregate(@event.StartTime, @event.EndTime, @event.UserId, @event.PartnerId);
    }
}