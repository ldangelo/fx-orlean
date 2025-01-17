using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

public class VideoConferenceAddedToPartnerEvent : Event
{
    public VideoConferenceAddedToPartnerEvent(Guid? conferenceId)
    {
        ConferenceId = conferenceId;
    }

    public Guid? ConferenceId { get; init; }
}