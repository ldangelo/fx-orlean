using Marten;
using Orleankka;
using Orleankka.Meta;
using UI.Aggregates;

namespace org.fortium.fx.Aggregates;

public abstract class EventSourcedActor : DispatchActorGrain
{
    StreamRef<IEventEnvelope?>? stream;
    IDocumentStore eventStore;

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
                stream = this.GetStream($"{GetType().Name}-{Id}");
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

    IEventEnvelope? Wrap(Event @event)
    {
        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(@event.GetType());
        return (IEventEnvelope?) Activator.CreateInstance(envelopeType, Id, @event);

    }

    async void Save(Event @event) {
        await using var session = eventStore.LightweightSession();
        Marten.Events.StreamAction streamAction = session.Events.Append(this.Id, @event);
       await session.SaveChangesAsync();
    }

    void Load() {

    }
}
