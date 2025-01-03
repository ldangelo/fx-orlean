using org.fortium.fx.Aggregates;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using UI.Grains.VideoConference;
using UI.Grains.VideoConference.Commands;

namespace UI.Aggregates.VideoConference;

[Alias("UI.Aggregates.VideoConference.IVideoConferenceAggregate")]
public interface IVideoConferenceAggregate : IActorGrain, IGrainWithStringKey
{
}

[Serializable]
[GenerateSerializer]
public class GetVideoConferenceDetails : Query<VideoConferenceAggregate, VideoConferenceSnapshot>
{
}

[MayInterleave(nameof(Interleave))]
public class VideoConferenceAggregate : EventSourcedActor, IVideoConferenceAggregate
{
    private DateTime _conferenceEndTime;
    private Guid _conferenceId;
    private DateTime _conferenceStartTime;
    private string _partnerId;
    private string _userId;

    private async void On(VideoConferenceCreatedEvent e)
    {
        _conferenceId = e.ConferenceId;
        _conferenceStartTime = e.StartTime;
        _conferenceEndTime = e.EndTime;
        _userId = e.UserId;
        _partnerId = e.PartnerId;
    }

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetVideoConferenceDetails;
    }

    private IEnumerable<Event> Handle(CreateVideoConferenceCommand command)
    {
        yield return new VideoConferenceCreatedEvent(command.conferenceId, command.startTime, command.endTime,
            command.userId,
            command.partnerId);
    }
}