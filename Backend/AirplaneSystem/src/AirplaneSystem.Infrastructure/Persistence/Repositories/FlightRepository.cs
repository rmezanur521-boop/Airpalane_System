using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class FlightRepository : Repository<Flight>, IFlightRepository
{
    public FlightRepository(AppDbContext context) : base(context) { }

    public async Task<List<Flight>> SearchFlightsAsync(FlightSearchCriteria criteria, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(f => f.Airline)
            .Include(f => f.Aircraft)
            .Include(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .Where(f => f.Route.OriginAirport.IataCode == criteria.OriginIata
                && f.Route.DestinationAirport.IataCode == criteria.DestinationIata
                && f.DepartureTime.Date == criteria.DepartureDate.ToDateTime(TimeOnly.MinValue).Date
                && f.Status != FlightStatus.Cancelled);

        query = criteria.SeatClass switch
        {
            SeatClass.Economy => query.Where(f => f.AvailableEconomySeats >= criteria.PassengerCount),
            SeatClass.Business => query.Where(f => f.AvailableBusinessSeats >= criteria.PassengerCount),
            SeatClass.First => query.Where(f => f.AvailableFirstClassSeats >= criteria.PassengerCount),
            _ => query
        };

        return await query.OrderBy(f => f.DepartureTime).ToListAsync(ct);
    }

    public async Task<Flight?> GetWithSeatsAsync(Guid flightId, CancellationToken ct = default) =>
        await _dbSet.Include(f => f.Seats)
            .FirstOrDefaultAsync(f => f.Id == flightId, ct);

    public async Task<Flight?> GetWithDetailsAsync(Guid flightId, CancellationToken ct = default) =>
        await _dbSet
            .Include(f => f.Airline)
            .Include(f => f.Aircraft)
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .FirstOrDefaultAsync(f => f.Id == flightId, ct);

    public async Task<int> GetAvailableSeatsCountAsync(Guid flightId, SeatClass seatClass, CancellationToken ct = default)
    {
        var flight = await _dbSet.FindAsync(new object[] { flightId }, ct);
        return flight == null ? 0 : flight.GetAvailableSeatsForClass(seatClass);
    }

    public async Task<bool> FlightNumberExistsAsync(string flightNumber, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(f => f.FlightNumber == flightNumber, ct);

    public async Task<Aircraft?> GetAircraftByIdAsync(Guid aircraftId, CancellationToken ct = default) =>
        await _context.Set<Aircraft>().FindAsync(new object[] { aircraftId }, ct);
}
