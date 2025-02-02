using Marten;
using Marten.Events;
using Orleankka;
using Orleankka.Meta;
using Serilog;
using UI.Aggregates;

namespace org.fortium.fx.Aggregates;

public abstract class EventSourcedActor : DispatchActorGrain
{
    private readonly IDocumentStore eventStore;
    private IDocumentSession _eventSession;
    private StreamAction _eventStream;
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
                _eventSession = eventStore.LightweightSession();
                /*
                try
                {
                    Log.Information("Starting eventstrem for {Type} with {Id}", GetType(), Id);
                    _eventStream =
                        _eventSession.Events.StartStream(GetType(), Id, new StreamStartEvent(Id, DateTime.UtcNow));
                    await _eventSession.SaveChangesAsync();
                }
                catch (DocumentAlreadyExistsException)
                {
                    Log.Information("Stream already exists for aggregate {Id}", Id);
                }
                catch (EventStreamUnexpectedMaxEventIdException)
                {
                    Log.Information("Stream already exists for aggregate {Id}", Id);
                }*/
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
        Log.Information("Saving event {@event} for aggregate {Id}", @event, Id);
        // append events to this grains event state
        _eventSession.Events.Append(Id, @event);
        await _eventSession.SaveChangesAsync();
    }

    private void Load()
    {
    }

    //
    // psudoevent to mark the beginning of a event stream
    public record StreamStartEvent(string aggregateId, DateTime startTime);
}