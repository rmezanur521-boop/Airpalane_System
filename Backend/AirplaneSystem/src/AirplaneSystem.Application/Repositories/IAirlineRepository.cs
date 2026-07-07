using AirplaneSystem.Domain.Entities.Flights;

namespace AirplaneSystem.Application.Repositories;

public interface IAirlineRepository : IRepository<Airline>
{
    Task<Airline?> GetByIataCodeAsync(string iataCode, CancellationToken ct = default);
    Task<Airline?> GetByIdWithImagesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Airline>> GetAllWithImagesAsync(CancellationToken ct = default);
}