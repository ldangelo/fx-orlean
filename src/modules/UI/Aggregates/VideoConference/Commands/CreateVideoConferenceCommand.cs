using Orleankka.Meta;

namespace UI.Grains.VideoConference.Commands;

public record CreateVideoConferenceCommand(DateTime startTime, DateTime endTime, string userId, string partnerId)
    : Command
{
}