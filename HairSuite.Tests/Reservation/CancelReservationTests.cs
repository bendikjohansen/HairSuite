using FluentAssertions;
using HairSuite.Domain;
using Xunit;

namespace HairSuite.Tests.Reservation;

public class CancelReservationTests : ReservationTestBase
{
    [Fact]
    public void CancelReservation_ReservationIsTentative_ReservationIsCancelled()
    {
        var reservation = MakeReservation();

        reservation.Cancel(reservation.ReservationId);

        var events = reservation.DequeueUncommittedEvents();
        events.Should().HaveCount(2);
        events.Last().Should().BeAssignableTo<Events.ReservationCancelled>();
    }

    [Fact]
    public void CancelReservation_ReservationIsConfirmed_ReservationIsCancelled()
    {
        var reservation = MakeReservation();
        reservation.Confirm(reservation.ReservationId, _ => false);

        reservation.Cancel(reservation.ReservationId);

        var events = reservation.DequeueUncommittedEvents();
        events.Should().HaveCount(3);
        events.Last().Should().BeAssignableTo<Events.ReservationCancelled>();
    }

    [Fact]
    public void CancelReservation_ReservationIsAlreadyCancelled_ThrowsDomainException()
    {
        var reservation = MakeReservation();
        reservation.Cancel(reservation.ReservationId);

        void CancelReservation() => reservation.Cancel(reservation.ReservationId);

        Assert.Throws<DomainException>(CancelReservation);
    }
}
