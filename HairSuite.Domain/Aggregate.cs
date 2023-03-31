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

    private void Enqueue(object @event) => _uncommittedEvents.Enqueue(@event);

    protected void HandleEvent<TEvent>(TEvent @event, Action<TEvent> apply) where TEvent : notnull {
        Enqueue(@event);
        apply(@event);
    }
}

public record AggregateId(Guid Value)
{
    public static implicit operator Guid(AggregateId id) => id.Value;
}
