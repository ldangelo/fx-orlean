using Serilog;
using UI.Aggregates.VideoConference;
using UI.Grains.Users;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Users.Events;

public record VideoConferenceAddedToUserEvent(string conferenceId): IEvent {};

public class VideoConferenceAddedToUserEventHandler: IEventHandler<UserAggregate, VideoConferenceAddedToUserEvent>
{
    public UserAggregate Apply(IEventHandlerContext<UserAggregate> context, VideoConferenceAddedToUserEvent @event)
    {
        Log.Information("VideoConferenceAddedToUserEventHandler");
        context.Aggregate.VideoConferences.Add(@event.conferenceId);
        return context.Aggregate;
    }
}