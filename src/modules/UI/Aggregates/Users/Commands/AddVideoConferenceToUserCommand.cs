using Orleankka.Meta;

namespace UI.Aggregates.Users.Commands;

public class AddVideoConferenceToUserCommand : Command
{
    private string ConferenceId {get; init; }= "";

    public AddVideoConferenceToUserCommand(string conferenceId) {
        ConferenceId = conferenceId;
    }
}
