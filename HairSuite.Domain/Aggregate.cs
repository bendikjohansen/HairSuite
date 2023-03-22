namespace HairSuite.Domain;

public abstract class Aggregate
{
    public long Version { get; set; }

    [NonSerialized] private readonly Queue<object> _uncommittedEvents = new();

    public object[] DequeueUncommittedEvents()
    {
        var events = _uncommittedEvents.ToArray();
        _uncommittedEvents.Clear();
        return events;
    }

    protected void Enqueue(object @event) => _uncommittedEvents.Enqueue(@event);
}

public record AggregateId(Guid Value);
