using Orleankka;
using Orleankka.Meta;
using UI.Aggregates;

namespace org.fortium.fx.Aggregates;

public abstract class EventSourcedActor : DispatchActorGrain
{
    StreamRef<UI.Aggregates.IEventEnvelope>? stream;

    public override Task<object> Receive(object message)
    {
        switch (message)
        {
            case Activate _:
                stream = System.StreamOf<IEventEnvelope>("conferences",$"{GetType().Name}-{Id}");
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
    private Task Project(Event @event)
    {
        var envelope = Wrap(@event);

        return stream.Publish(envelope);
    }

    IEventEnvelope Wrap(Event @event)
    {
        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(@event.GetType());
        return (IEventEnvelope) Activator.CreateInstance(envelopeType, Id, @event);

    }
}
