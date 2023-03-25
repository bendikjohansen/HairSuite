using HairSuite.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HairSuite;

[ApiController]
[Route("/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IApplicationService<Reservation> _service;

    public ReservationsController(IApplicationService<Reservation> service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Query(Guid id, IDocumentSession documentSession)
    {
        var result = await documentSession.Json.FindByIdAsync<Reservation>(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Query(IDocumentSession documentSession) =>
        Ok(await documentSession.Query<Reservation>().ToListAsync());

    [HttpPost("make-tentative")]
    public async Task<IActionResult> MakeTentative(Commands.V1.MakeTentativeReservation command) =>
        Ok(await _service.Handle(command));

    [HttpPatch("confirm")]
    public async Task<IActionResult> ConfirmReservation(Commands.V1.ConfirmReservation command) =>
        Ok(await _service.Handle(command));

    [HttpPatch("reschedule")]
    public async Task<IActionResult> RescheduleReservation(Commands.V1.RescheduleReservation command) =>
        Ok(await _service.Handle(command));

    [HttpDelete("cancel")]
    public async Task<IActionResult> CancelReservation(Commands.V1.CancelReservation command) =>
        Ok(await _service.Handle(command));
}

public class ReservationService : IApplicationService<Reservation>
{
    private readonly IDocumentSession _documentSession;

    public ReservationService(IDocumentSession documentSession) => _documentSession = documentSession;

    public async Task<Reservation> Handle(object command)
    {
        var stream = command switch
        {
            Commands.V1.MakeTentativeReservation cmd => Create(cmd),
            Commands.V1.ConfirmReservation cmd => await GetAndUpdate(cmd.Id,
                reservation => reservation.Confirm(cmd.Id, IsDateReserved)),
            Commands.V1.RescheduleReservation cmd => await GetAndUpdate(cmd.Id,
                reservation => reservation.Reschedule(cmd.Id, cmd.Date)),
            Commands.V1.CancelReservation cmd => await GetAndUpdate(cmd.Id, reservation => reservation.Cancel(cmd.Id)),
            _ => throw new NotSupportedException("Command is not supported"),
        };

        await _documentSession.SaveChangesAsync();

        return (await _documentSession.LoadAsync<Reservation>(stream))!;
    }

    private Guid Create(Commands.V1.MakeTentativeReservation command)
    {
        var (id, userId, date) = command;
        var reservation = Reservation.MakeTentative(id, userId, date);

        var events = reservation.DequeueUncommittedEvents();
        var stream = _documentSession.Events.StartStream<Reservation>(id, events);
        return stream.Id;
    }

    private async Task<Guid> GetAndUpdate(Guid id, Action<Reservation> action)
    {
        var reservation = await RequireReservation(id);
        action(reservation);
        return AppendChangesToStream(reservation);
    }

    private async Task<Reservation> RequireReservation(Guid id) =>
        await _documentSession.Events.AggregateStreamAsync<Reservation>(id) ??
        throw new Exception($"No such reservation found: {id}");

    private Guid AppendChangesToStream(Reservation reservation)
    {
        var events = reservation.DequeueUncommittedEvents();
        var nextVersion = reservation.Version + events.Length;
        var stream = _documentSession.Events.Append(reservation.ReservationId.Value, nextVersion, events);
        return stream.Id;
    }

    private bool IsDateReserved(ReservationId id, ReservationDate date) =>
        _documentSession.Query<Reservation>()
            .Any(other => other.Date.Value == date.Value && other.Status == ReservationStatus.Booked);
}

public interface IApplicationService<T>
{
    Task<T> Handle(object command);
}

public static class Commands
{
    public static class V1
    {
        public record MakeTentativeReservation(Guid Id, Guid UserId, DateTime Date);

        public record ConfirmReservation(Guid Id);

        public record RescheduleReservation(Guid Id, DateTime Date);

        public record CancelReservation(Guid Id);
    }
}
