namespace HairSuite.Tests.Reservation;

public abstract class ReservationTestBase
{
    protected static readonly Guid DefaultReservationId = Guid.NewGuid();

    protected static Domain.Reservation MakeReservation() =>
        Domain.Reservation.MakeTentative(DefaultReservationId, Guid.NewGuid(), DateTime.Now);
}
