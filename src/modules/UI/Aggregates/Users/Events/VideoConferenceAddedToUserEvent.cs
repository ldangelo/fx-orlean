using Orleankka.Meta;

namespace UI.Aggregates.Users.Events;

public record VideoConferenceAddedToUserEvent(string conferenceId) : Event
{
};