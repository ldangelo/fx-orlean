using UI.Aggregates.VideoConference;
using Whaally.Domain.Abstractions;

namespace UI.Grains.VideoConference;

public sealed record class VideoConferenceSnapshot : ISnapshot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public String? PartnerId { get; set; }
    public String? UserId { get; set; }
}


public class VideoConferenceSnapshotFactory: ISnapshotFactory<VideoConferenceAggregate, VideoConferenceSnapshot>
{
    public VideoConferenceSnapshot Instantiate(VideoConferenceAggregate aggregate)
    {
        var snapshot = new VideoConferenceSnapshot();
        snapshot.StartTime = aggregate.StartTime;
        snapshot.EndTime = aggregate.EndTime;
        snapshot.UserId = aggregate.UserId;
        snapshot.PartnerId = aggregate.PartnerId;
        return snapshot;
    }
}