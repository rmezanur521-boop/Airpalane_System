using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IFlightService
{
    Task<FlightDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<FlightDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
    Task<FlightDto> CreateAsync(CreateFlightRequest request, CancellationToken ct = default);
    Task<FlightDto> UpdateAsync(Guid id, CreateFlightRequest request, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, UpdateFlightStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<SeatMapDto> GetSeatMapAsync(Guid flightId, CancellationToken ct = default);
}
