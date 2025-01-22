using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

[Serializable, GenerateSerializer]
public class VideoConferenceAddedToPartnerEvent : Event
{
    public VideoConferenceAddedToPartnerEvent(Guid? conferenceId)
    {
        ConferenceId = conferenceId;
    }

    public Guid? ConferenceId { get; init; }
}

