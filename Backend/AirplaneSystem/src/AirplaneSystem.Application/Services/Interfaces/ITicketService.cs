using AirplaneSystem.Application.DTOs.Tickets;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface ITicketService
{
    Task<List<TicketDto>> GetByBookingAsync(Guid bookingId, CancellationToken ct = default);
    Task<TicketDto> GetByTicketNumberAsync(string ticketNumber, CancellationToken ct = default);
    Task<byte[]> GeneratePdfAsync(string ticketNumber, CancellationToken ct = default);
    Task<byte[]> GenerateBoardingPassAsync(string ticketNumber, CancellationToken ct = default);
    Task CheckInAsync(string ticketNumber, Guid userId, CancellationToken ct = default);
    Task GenerateAndPersistTicketsAsync(Guid bookingId, CancellationToken ct = default);
}
