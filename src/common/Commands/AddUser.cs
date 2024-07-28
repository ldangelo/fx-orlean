
using FluentResults;
using org.fortium.fx.common.Events;
using org.fortium.fx.common.Types;
using Whaally.Domain.Abstractions.Command;

namespace org.fortium.fx.commands;

[Immutable, GenerateSerializer]
public record AddUser(Guid id, string userName): ICommand;

public class AddUserHandler : ICommandHandler<User, AddUser> {

    public IResultBase Evaluate(ICommandHandlerContext<User> context, AddUser command)
    {
        context.StageEvent(new UserCreatedEvent(new User()));
        return Result.Ok();
    }
}
