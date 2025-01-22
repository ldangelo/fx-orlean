using Orleankka.Meta;

namespace UI.Aggregates.VideoConference.Events;

[Serializable, GenerateSerializer]
public class VideoConferenceCreatedEvent : Event
{
    public VideoConferenceCreatedEvent(
        Guid conferenceId,
        DateTime commandStartTime,
        DateTime commandEndTime,
        string commandUserId,
        string commandPartnerId
    )
    {
        ConferenceId = conferenceId;
        StartTime = commandStartTime;
        EndTime = commandEndTime;
        UserId = commandUserId;
        PartnerId = commandPartnerId;
    }

    [Id(0)]
    public Guid ConferenceId { get; }

    [Id(1)]
    public DateTime StartTime { get; set; }

    [Id(2)]
    public DateTime EndTime { get; set; }

    [Id(3)]
    public string UserId { get; set; } = "";

    [Id(4)]
    public string PartnerId { get; set; } = "";
}
