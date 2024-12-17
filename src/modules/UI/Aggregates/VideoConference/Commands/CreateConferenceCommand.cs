using FluentResults;
using UI.Aggregates.VideoConference;
using Whaally.Domain.Abstractions;

namespace UI.Grains.VideoConference.Commands;

public record CreateConferenceCommand(DateTime startTime, DateTime endTime, string userId, string partnerId): ICommand
{
    
}

public class CreateConferenceCommandHandler : ICommandHandler<VideoConferenceAggregate, CreateConferenceCommand>
{
    public IResultBase Evaluate(ICommandHandlerContext<VideoConferenceAggregate> context, CreateConferenceCommand command)
    {
        var result = new Result();
        
        context.StageEvent(new ConferenceCreatedEvent(command.startTime, command.endTime, command.userId, command.partnerId));

        return result;
    }
}