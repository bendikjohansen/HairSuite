using FluentAssertions;
using HairSuite.Domain;
using Xunit;

namespace HairSuite.Tests.Reservation;

public class CreateReservationTests : ReservationTestBase
{
    [Fact]
    public void MakeTentative_EverythingIsOk_ReservationCreatedEventIsQueued()
    {
        var reservation = MakeReservation();

        var events = reservation.DequeueUncommittedEvents();

        events.Should().ContainSingle();
        events.Single().Should().BeAssignableTo<Events.ReservationRequested>();
    }
}
