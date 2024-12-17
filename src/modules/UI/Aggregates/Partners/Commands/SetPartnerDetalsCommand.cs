using FluentResults;
using UI.Aggregates.Partners.Events;
using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Commands;

[Immutable]
[GenerateSerializer]
public record SetPartnerDetalsCommand(string emailAddress, string firstName, string lastName): ICommand 
{}

public class SetPartnerDetailsHandler: ICommandHandler<PartnerAggregate, SetPartnerDetalsCommand> {
    public IResultBase Evaluate(ICommandHandlerContext<PartnerAggregate> context, SetPartnerDetalsCommand command) {
      var result = new Result();

      context.StageEvent(new PartnerDetailsSetEvent(command.emailAddress, command.firstName, command.lastName));
      return result;
    }
  }
