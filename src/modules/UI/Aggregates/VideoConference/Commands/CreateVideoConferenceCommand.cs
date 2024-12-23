using Orleankka.Meta;

namespace UI.Aggregates.VideoConference.Commands;

[Serializable]
[GenerateSerializer]
public record CreateVideoConferenceCommand(
    Guid conferenceId,
    DateTime startTime,
    DateTime endTime,
    string userId,
    string partnerId)
    : Command
{
}
