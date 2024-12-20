using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using UI.Grains.VideoConference;

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
public class VideoConferenceAggregate : DispatchActorGrain, IVideoConferenceAggregate
{
    public DateTime EndTime;
    public string PartnerId;
    public DateTime StartTime;
    public string UserId;

    public VideoConferenceAggregate(DateTime eventStartTime, DateTime eventEndTime, string eventUserId,
        string eventPartnerId)
    {
        StartTime = eventStartTime;
        EndTime = eventEndTime;
        UserId = eventUserId;
        PartnerId = eventPartnerId;
    }

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetVideoConferenceDetails;
    }
}
