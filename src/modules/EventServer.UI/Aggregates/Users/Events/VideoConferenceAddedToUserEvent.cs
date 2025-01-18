using Orleankka.Meta;

namespace UI.Aggregates.Users.Events;

[Serializable, GenerateSerializer]
public record VideoConferenceAddedToUserEvent : Event
{
    public VideoConferenceAddedToUserEvent(Guid? conferenceId)
    {
        ConferenceId = conferenceId;
    }

    [Id(0)]
    public Guid? ConferenceId { get; }
}
