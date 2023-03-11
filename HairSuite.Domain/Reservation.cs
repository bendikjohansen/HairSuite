namespace HairSuite.Domain;

public class Reservation : Aggregate
{
    public Guid Id
    {
        get => ReservationId.Value;
        set {}
    }
    public ReservationId ReservationId { get; set; }
    public HairdresserId UserId { get; set; }
    public ReservationDate Date { get; set; }
    public ReservationStatus Status { get; set; }

    public Reservation() { } // For serialization

    private Reservation(Guid id, Guid hairdresserId, DateTime date)
    {
        var @event = new Events.ReservationRequested(id, hairdresserId, date);
        Enqueue(@event);
        Apply(@event);
    }

    public static Reservation MakeTentative(Guid id, Guid hairdresserId, DateTime date) => new(id, hairdresserId, date);

    public void Confirm(Guid id)
    {
        if (Status != ReservationStatus.Tentative)
        {
            throw new DomainException("Only requested reservations can be confirmed.");
        }

        var @event = new Events.ReservationConfirmed(id);
        Enqueue(@event);
        Apply(@event);
    }

    public void Cancel(Guid id)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new DomainException("Can not cancel a reservation that is already cancelled.");
        }

        var @event = new Events.ReservationCancelled(id);
        Enqueue(@event);
        Apply(@event);
    }

    public void Reschedule(Guid id, DateTime date)
    {
        var @event = new Events.ReservationRescheduled(id, date);
        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(Events.ReservationRequested @event)
    {
        ReservationId = new ReservationId(@event.Id);
        UserId = new HairdresserId(@event.HairdresserId);
        Date = new ReservationDate(@event.Date);
        Status = ReservationStatus.Tentative;
    }

    public void Apply(Events.ReservationConfirmed _) => Status = ReservationStatus.Booked;

    public void Apply(Events.ReservationCancelled _) => Status = ReservationStatus.Cancelled;

    public void Apply(Events.ReservationRescheduled @event) => Date = new ReservationDate(@event.Date);
}

public record ReservationId(Guid Value) : AggregateId(Value);

public record HairdresserId(Guid Value);

public record ReservationDate(DateTime Value);

public enum ReservationStatus
{
    Tentative,
    Booked,
    Cancelled
}

public static class Events
{
    public record ReservationRequested(Guid Id, Guid HairdresserId, DateTime Date);

    public record ReservationConfirmed(Guid Id);

    public record ReservationCancelled(Guid Id);

    public record ReservationRescheduled(Guid Id, DateTime Date);
}
