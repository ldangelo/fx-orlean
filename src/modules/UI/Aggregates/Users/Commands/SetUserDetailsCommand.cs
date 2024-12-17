using FluentResults;
using Whaally.Domain.Abstractions;

namespace UI.Grains.Users.Commands;

[Immutable]
[GenerateSerializer]
public record SetUserDetailsCommand(string emailAddress, string firstName, string lastName) : ICommand
{
}

public class SetPartnerDetailsHandler: ICommandHandler<UserAggregate, SetUserDetailsCommand> {
    public IResultBase Evaluate(ICommandHandlerContext<UserAggregate> context, SetUserDetailsCommand command) {
      var result = new Result();

      context.StageEvent(new UserDetailsSetEvent(command.emailAddress, command.firstName, command.lastName));
      return result;
    }
  }