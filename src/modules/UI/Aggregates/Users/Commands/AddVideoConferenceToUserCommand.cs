using FluentResults;
using UI.Aggregates.Users.Events;
using UI.Grains.Users;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Users.Commands;

public record AddVideoConferenceToUserCommand(String conferenceId): ICommand;

public class AddVideoConferenceToUserCommandHandler: ICommandHandler<UserAggregate, AddVideoConferenceToUserCommand>
{
    public IResultBase Evaluate(ICommandHandlerContext<UserAggregate> context, AddVideoConferenceToUserCommand command)
    {
        context.StageEvent(new VideoConferenceAddedToUserEvent(command.conferenceId));
        return Result.Ok();
    }
}