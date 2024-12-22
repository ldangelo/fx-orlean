using Orleankka.Meta;

namespace UI.Aggregates.Users.Commands;

[Serializable]
[GenerateSerializer]
public class AddVideoConferenceToUserCommand : Command
{
    public AddVideoConferenceToUserCommand(Guid conferenceId)
    {
        ConferenceId = conferenceId;
    }

    [Id(0)] public Guid? ConferenceId { get; }
}