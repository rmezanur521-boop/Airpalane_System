using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class FlightService : IFlightService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<FlightService> _logger;
    private readonly INotificationService _notification;

    public FlightService(IUnitOfWork uow, IMapper mapper, ILogger<FlightService> logger, INotificationService notification)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _notification = notification;
    }

    public async Task<FlightDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("Flight", id);
        return _mapper.Map<FlightDto>(flight);
    }

    public async Task<PagedResult<FlightDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var flights = await _uow.Flights.GetAllAsync(ct);
        var total = flights.Count;
        var items = flights
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => _mapper.Map<FlightDto>(f))
            .ToList();
        return PagedResult<FlightDto>.Create(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<FlightDto> CreateAsync(CreateFlightRequest request, CancellationToken ct = default)
    {
        if (await _uow.Flights.FlightNumberExistsAsync(request.FlightNumber, ct))
            throw new ConflictException($"Flight number '{request.FlightNumber}' already exists.");

        var aircraft = await _uow.Flights.GetAircraftByIdAsync(request.AircraftId, ct)
            ?? throw new NotFoundException("Aircraft", request.AircraftId);

        var flight = new Flight
        {
            FlightNumber = request.FlightNumber.ToUpperInvariant(),
            AirlineId = request.AirlineId,
            AircraftId = request.AircraftId,
            RouteId = request.RouteId,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            EconomyBasePrice = request.EconomyBasePrice,
            BusinessBasePrice = request.BusinessBasePrice,
            FirstClassBasePrice = request.FirstClassBasePrice,
            AirportFee = request.AirportFee,
            TaxPercentage = request.TaxPercentage,
            GateNumber = request.GateNumber,
            Status = FlightStatus.Scheduled,
            AvailableEconomySeats = aircraft.EconomySeats,
            AvailableBusinessSeats = aircraft.BusinessSeats,
            AvailableFirstClassSeats = aircraft.FirstClassSeats
        };

        await _uow.Flights.AddAsync(flight, ct);
        await _uow.SaveChangesAsync(ct);

        var created = await _uow.Flights.GetWithDetailsAsync(flight.Id, ct);
        _logger.LogInformation("Flight created: {FlightNumber}", flight.FlightNumber);
        return _mapper.Map<FlightDto>(created);
    }

    public async Task<FlightDto> UpdateAsync(Guid id, CreateFlightRequest request, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetByIdAsync(id, ct) ?? throw new NotFoundException("Flight", id);

        flight.DepartureTime = request.DepartureTime;
        flight.ArrivalTime = request.ArrivalTime;
        flight.EconomyBasePrice = request.EconomyBasePrice;
        flight.BusinessBasePrice = request.BusinessBasePrice;
        flight.FirstClassBasePrice = request.FirstClassBasePrice;
        flight.AirportFee = request.AirportFee;
        flight.TaxPercentage = request.TaxPercentage;
        flight.GateNumber = request.GateNumber;

        _uow.Flights.Update(flight);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<FlightDto>(await _uow.Flights.GetWithDetailsAsync(id, ct));
    }

    public async Task UpdateStatusAsync(Guid id, UpdateFlightStatusRequest request, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetByIdAsync(id, ct) ?? throw new NotFoundException("Flight", id);
        var oldStatus = flight.Status;
        flight.Status = request.Status;
        if (request.GateNumber != null) flight.GateNumber = request.GateNumber;
        _uow.Flights.Update(flight);
        await _uow.SaveChangesAsync(ct);

        if (request.Status == FlightStatus.Delayed || request.Status == FlightStatus.Cancelled)
        {
            try
            {
                if (request.Status == FlightStatus.Cancelled)
                    await _notification.SendFlightCancellationAlertAsync(id, ct);
                else
                    await _notification.SendFlightDelayAlertAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send flight status alert for flight {Id}", id);
            }
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetByIdAsync(id, ct) ?? throw new NotFoundException("Flight", id);
        flight.IsDeleted = true;
        _uow.Flights.Update(flight);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<SeatMapDto> GetSeatMapAsync(Guid flightId, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetWithSeatsAsync(flightId, ct)
            ?? throw new NotFoundException("Flight", flightId);

        return new SeatMapDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            Seats = flight.Seats.Select(s => _mapper.Map<SeatDto>(s)).ToList()
        };
    }
}
