using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.Repositories;

public interface IFlightRepository : IRepository<Flight>
{
    Task<List<Flight>> SearchFlightsAsync(FlightSearchCriteria criteria, CancellationToken ct = default);
    Task<Flight?> GetWithSeatsAsync(Guid flightId, CancellationToken ct = default);
    Task<Flight?> GetWithDetailsAsync(Guid flightId, CancellationToken ct = default);
    Task<int> GetAvailableSeatsCountAsync(Guid flightId, SeatClass seatClass, CancellationToken ct = default);
    Task<bool> FlightNumberExistsAsync(string flightNumber, CancellationToken ct = default);
    Task<Aircraft?> GetAircraftByIdAsync(Guid aircraftId, CancellationToken ct = default);
}
