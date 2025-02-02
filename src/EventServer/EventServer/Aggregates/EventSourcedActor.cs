using Marten;
using Orleankka;
using Orleankka.Meta;
using UI.Aggregates;

namespace org.fortium.fx.Aggregates;

public abstract class EventSourcedActor : DispatchActorGrain
{
    private readonly IDocumentStore eventStore;
    private StreamRef<IEventEnvelope?>? stream;

    protected EventSourcedActor(IDocumentStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public abstract StreamRef<IEventEnvelope?> GetStream(string id);
    protected abstract Task SaveState();

    public override Task<object> Receive(object message)
    {
        switch (message)
        {
            case Activate _:
                stream = GetStream($"{GetType().Name}-{Id}");
                Load();
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

        Save(@event);

        return stream.Publish(envelope);
    }

    private IEventEnvelope? Wrap(Event @event)
    {
        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(@event.GetType());
        return (IEventEnvelope?)Activator.CreateInstance(envelopeType, Id, @event);
    }

    private async void Save(Event @event)
    {
        await using var session = eventStore.LightweightSession();

        // append events to this grains event state
        var streamAction = session.Events.StartStream(this.Id, @event);
        await session.SaveChangesAsync();
    }

    private void Load() { }
}

