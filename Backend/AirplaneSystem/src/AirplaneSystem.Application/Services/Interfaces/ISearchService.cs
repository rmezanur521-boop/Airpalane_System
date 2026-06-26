using AirplaneSystem.Application.DTOs.Flights;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface ISearchService
{
    Task<List<FlightSearchResult>> SearchOneWayAsync(FlightSearchRequest request, CancellationToken ct = default);
    Task<List<RoundTripResult>> SearchRoundTripAsync(FlightSearchRequest request, CancellationToken ct = default);
    Task<List<MultiCityResult>> SearchMultiCityAsync(MultiCitySearchRequest request, CancellationToken ct = default);
}
