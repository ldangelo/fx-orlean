namespace EventServer.Aggregates.VideoConference.Events;

public interface IVideoConferenceEvent
{
}

[Serializable]
public record VideoConferenceCreatedEvent(
        Guid conferenceId,
        DateTime commandStartTime,
        DateTime commandEndTime,
        string commandUserId,
        string commandPartnerId
    ): IVideoConferenceEvent;
