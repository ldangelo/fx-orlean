using FluentResults;
using UI.Aggregates.Partners.Events;
using UI.Grains.Partners;
using Whaally.Domain.Abstractions;

namespace UI.Aggregates.Partners.Commands;

public record AddVideoConferenceToPartnerCommand(string conferenceId): ICommand;

public class AddVideoConferenceToPartnerCommandHandler: ICommandHandler<PartnerAggregate, AddVideoConferenceToPartnerCommand>
{
    public IResultBase Evaluate(ICommandHandlerContext<PartnerAggregate> context, AddVideoConferenceToPartnerCommand command)
    {
       context.StageEvent(new ConferenceAddedToPartnerEvent(command.conferenceId));
       return Result.Ok();
    }
}