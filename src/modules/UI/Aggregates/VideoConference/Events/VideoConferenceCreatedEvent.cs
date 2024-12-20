using Orleankka.Meta;

namespace UI.Grains.VideoConference.Commands;

public class VideoConferenceCreatedEvent : Event
{
    public VideoConferenceCreatedEvent(DateTime commandStartTime, DateTime commandEndTime, string commandUserId,
        string commandPartnerId)
    {
        StartTime = commandStartTime;
        EndTime = commandEndTime;
        UserId = commandUserId;
        PartnerId = commandPartnerId;
    }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string UserId { get; set; } = "";
    public string PartnerId { get; set; } = "";
}