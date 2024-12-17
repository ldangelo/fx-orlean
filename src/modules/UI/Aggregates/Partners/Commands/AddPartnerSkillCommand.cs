using FluentResults;
using UI.Aggregates.Partners.Events;
using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Commands;

[Immutable]
[GenerateSerializer]
public record AddPartnerSkillCommand(string skill): ICommand
{}

public class PartnerSkillAddHandler: ICommandHandler<PartnerAggregate, AddPartnerSkillCommand> {
  public IResultBase Evaluate(ICommandHandlerContext<PartnerAggregate> context, AddPartnerSkillCommand command) {
    context.StageEvent(new PartnerSkillAddedEvent(command.skill));
    return Result.Ok();
  }
}
