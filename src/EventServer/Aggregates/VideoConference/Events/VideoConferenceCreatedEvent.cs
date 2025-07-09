using Fortium.Types;

namespace EventServer.Aggregates.VideoConference.Events;

public interface IVideoConferenceEvent
{
}

[Serializable]
public record VideoConferenceCreatedEvent(
    Guid ConferenceId,
    DateTime StartTime,
    DateTime EndTime,
    string UserId,
    string PartnerId,
    RateInformation RateInformation
) : IVideoConferenceEvent;
