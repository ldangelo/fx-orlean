using Marten;

namespace EventServer.Aggregates.VideoConference;

public interface IVideoConferenceAggregate
{
}

public class VideoConferenceAggregate : IVideoConferenceAggregate
{
    private Guid _conferenceId;
    private DateTime _conferenceEndTime;
    private DateTime _conferenceStartTime;
    private string? _partnerId;
    private string? _userId;

    public VideoConferenceAggregate(IDocumentStore eventStore)
    {
    }
}
