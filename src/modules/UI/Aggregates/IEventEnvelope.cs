using Orleankka.Meta;

namespace UI.Aggregates;

public interface IEventEnvelope {}

[Serializable, GenerateSerializer]
public class EventEnvelope<T>: IEventEnvelope where T: Event
{
    [Id(0)]
    public string Stream { get; }

    [Id(1)]
    public T Event { get; }

    public EventEnvelope(string stream, T @event)
    {
        Stream = stream;
        Event = @event;
    }
}
