using Orleankka.Meta;

namespace UI.Aggregates.Users.Events;

public record VideoConferenceAddedToUserEvent : Event
{
    public VideoConferenceAddedToUserEvent(Guid? conferenceId)
    {
        ConferenceId = conferenceId;
    }

    public Guid? ConferenceId { get; }
}