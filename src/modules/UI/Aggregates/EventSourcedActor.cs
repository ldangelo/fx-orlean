using Microsoft.CodeAnalysis;
using Orleankka;
using Orleankka.Meta;
using UI.Grains.Users;

namespace org.fortium.fx.Aggregates;

public abstract class EventSourcedActor : DispatchActorGrain, IEventSourcedActor
{
    public override Task<object> Receive(object message)
    {
        switch (message)
        {
            case Activate _:
                return Result(Done);

            case Command cmd:
                return HandleCommand(cmd);

            default:
                return base.Receive(message);
        }
    }

    private async Task<object> HandleCommand(Command cmd)
    {
        var events = Dispatcher.DispatchResult<IEnumerable<Event>>(this, cmd);

        foreach (var @event in events)
        {
            Dispatcher.Dispatch(this, @event);
            await Project(@event);
        }

        return events;
    }

    //
    // TODO: Project events into EventDb or MartinDb
    protected Task Project(Event @event)
    {
        return Task.CompletedTask;
    }
}