using FluentAssertions;
using HairSuite.Domain;
using Xunit;

namespace HairSuite.Tests.Reservation;

public class ConfirmReservationTests : ReservationTestBase
{
    [Fact]
    public void ConfirmReservation_EverythingIsOk_ReservationIsConfirmed()
    {
        var reservation = MakeReservation();

        reservation.Confirm(DefaultReservationId, _ => false);

        var events = reservation.DequeueUncommittedEvents();
        events.Should().HaveCount(2);
        events.Last().Should().BeAssignableTo<Events.ReservationConfirmed>();
    }

    [Fact]
    public void ConfirmReservation_ReservationIsAlreadyConfirmed_ThrowsDomainException()
    {
        var reservation = MakeReservation();
        reservation.Confirm(DefaultReservationId, _ => false);

        void Confirm() => reservation.Confirm(DefaultReservationId, _ => false);

        Assert.Throws<DomainException>(Confirm);
    }

    [Fact]
    public void ConfirmReservation_ReservationIsCancelled_ThrowsDomainException()
    {
        var reservation = MakeReservation();
        reservation.Cancel(DefaultReservationId);

        void Confirm() => reservation.Confirm(DefaultReservationId, _ => false);

        Assert.Throws<DomainException>(Confirm);
    }

    [Fact]
    public void ConfirmReservation_ReservationDateIsBusy_ThrowsDomainException()
    {
        var reservation = MakeReservation();

        void Confirm() => reservation.Confirm(DefaultReservationId, _ => true);

        Assert.Throws<DomainException>(Confirm);
    }
}
