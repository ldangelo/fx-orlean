using Orleankka.Meta;

namespace UI.Aggregates.VideoConference.Events;

public class VideoConferenceCreatedEvent : Event
{
    public VideoConferenceCreatedEvent(Guid conferenceId, DateTime commandStartTime, DateTime commandEndTime,
        string commandUserId,
        string commandPartnerId)
    {
        ConferenceId = conferenceId;
        StartTime = commandStartTime;
        EndTime = commandEndTime;
        UserId = commandUserId;
        PartnerId = commandPartnerId;
    }

    public Guid ConferenceId { get; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string UserId { get; set; } = "";
    public string PartnerId { get; set; } = "";
}
