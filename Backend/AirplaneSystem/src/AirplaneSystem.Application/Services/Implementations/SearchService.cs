using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IUnitOfWork uow, IMapper mapper, ICacheService cache, ILogger<SearchService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<FlightSearchResult>> SearchOneWayAsync(FlightSearchRequest request, CancellationToken ct = default)
    {
        await ValidateAirports(request.OriginIata, request.DestinationIata, ct);

        var cacheKey = $"search:oneway:{request.OriginIata}:{request.DestinationIata}:{request.DepartureDate}:{request.SeatClass}:{request.Passengers.Total}";
        if (_cache.TryGet<List<FlightSearchResult>>(cacheKey, out var cached) && cached != null)
            return cached;

        var criteria = new FlightSearchCriteria
        {
            OriginIata = request.OriginIata.ToUpperInvariant(),
            DestinationIata = request.DestinationIata.ToUpperInvariant(),
            DepartureDate = request.DepartureDate,
            SeatClass = request.SeatClass,
            PassengerCount = request.Passengers.Total,
            MaxStops = request.MaxStops
        };

        var flights = await _uow.Flights.SearchFlightsAsync(criteria, ct);
        var results = flights.Select(f =>
        {
            var r = _mapper.Map<FlightSearchResult>(f);
            r.RequestedClass = request.SeatClass;
            var basePrice = f.GetBasePriceForClass(request.SeatClass);
            var tax = basePrice * (f.TaxPercentage / 100);
            r.TotalPrice = Math.Round((basePrice + tax + f.AirportFee) * request.Passengers.Total, 2);
            r.Stops = 0;
            return r;
        }).ToList();

        results = SortResults(results, request.SortBy, request.SortDescending);
        if (request.MaxPrice.HasValue)
            results = results.Where(r => r.TotalPrice <= request.MaxPrice.Value).ToList();

        _cache.Set(cacheKey, results, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Flight search: {Origin}→{Dest} {Date}, found {Count} flights",
            request.OriginIata, request.DestinationIata, request.DepartureDate, results.Count);

        return results;
    }

    public async Task<List<RoundTripResult>> SearchRoundTripAsync(FlightSearchRequest request, CancellationToken ct = default)
    {
        if (!request.ReturnDate.HasValue) throw new ValidationException("returnDate", "Return date is required for round-trip search.");

        var outbound = await SearchOneWayAsync(request, ct);
        var returnRequest = new FlightSearchRequest
        {
            OriginIata = request.DestinationIata,
            DestinationIata = request.OriginIata,
            DepartureDate = request.ReturnDate.Value,
            Passengers = request.Passengers,
            SeatClass = request.SeatClass,
            MaxStops = request.MaxStops
        };
        var returnFlights = await SearchOneWayAsync(returnRequest, ct);

        var results = new List<RoundTripResult>();
        foreach (var out_ in outbound.Take(10))
            foreach (var ret in returnFlights.Take(10))
                results.Add(new RoundTripResult
                {
                    OutboundFlight = out_,
                    ReturnFlight = ret,
                    TotalPrice = out_.TotalPrice + ret.TotalPrice
                });

        return results.OrderBy(r => r.TotalPrice).Take(50).ToList();
    }

    public async Task<List<MultiCityResult>> SearchMultiCityAsync(MultiCitySearchRequest request, CancellationToken ct = default)
    {
        if (request.Legs.Count < 2) throw new ValidationException("legs", "Multi-city search requires at least 2 legs.");

        var legResults = new List<List<FlightSearchResult>>();
        foreach (var leg in request.Legs)
        {
            var legRequest = new FlightSearchRequest
            {
                OriginIata = leg.OriginIata,
                DestinationIata = leg.DestinationIata,
                DepartureDate = leg.DepartureDate,
                Passengers = request.Passengers,
                SeatClass = request.SeatClass
            };
            legResults.Add(await SearchOneWayAsync(legRequest, ct));
        }

        var results = new List<MultiCityResult>();
        if (legResults.All(l => l.Any()))
        {
            foreach (var firstFlight in legResults[0].Take(5))
                foreach (var secondFlight in legResults[1].Take(5))
                {
                    var r = new MultiCityResult { Flights = new List<FlightSearchResult> { firstFlight, secondFlight } };
                    for (int i = 2; i < legResults.Count; i++)
                        r.Flights.Add(legResults[i].FirstOrDefault()!);
                    r.TotalPrice = r.Flights.Where(f => f != null).Sum(f => f.TotalPrice);
                    results.Add(r);
                }
        }

        return results.OrderBy(r => r.TotalPrice).Take(20).ToList();
    }

    private async Task ValidateAirports(string origin, string destination, CancellationToken ct)
    {
        var originAirport = await _uow.Airports.GetByIataCodeAsync(origin.ToUpperInvariant(), ct);
        if (originAirport == null) throw new NotFoundException($"Airport with IATA code '{origin}' not found.");
        var destAirport = await _uow.Airports.GetByIataCodeAsync(destination.ToUpperInvariant(), ct);
        if (destAirport == null) throw new NotFoundException($"Airport with IATA code '{destination}' not found.");
    }

    private static List<FlightSearchResult> SortResults(List<FlightSearchResult> results, string sortBy, bool descending)
    {
        IOrderedEnumerable<FlightSearchResult> sorted = sortBy?.ToLowerInvariant() switch
        {
            "duration" => results.OrderBy(r => r.DurationMinutes),
            "departure" => results.OrderBy(r => r.DepartureTime),
            "arrival" => results.OrderBy(r => r.ArrivalTime),
            _ => results.OrderBy(r => r.TotalPrice)
        };
        return (descending ? sorted.Reverse() : sorted).ToList();
    }
}
