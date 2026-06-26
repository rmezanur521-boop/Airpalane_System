using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Tickets;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Tickets;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AirplaneSystem.Application.Services.Implementations;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPdfService _pdf;
    private readonly IQrCodeService _qrCode;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IUnitOfWork uow, IMapper mapper, IPdfService pdf,
        IQrCodeService qrCode, ILogger<TicketService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _pdf = pdf;
        _qrCode = qrCode;
        _logger = logger;
    }

    public async Task<List<TicketDto>> GetByBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var tickets = await _uow.Tickets.GetByBookingIdAsync(bookingId, ct);
        return tickets.Select(t => _mapper.Map<TicketDto>(t)).ToList();
    }

    public async Task<TicketDto> GetByTicketNumberAsync(string ticketNumber, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetByTicketNumberAsync(ticketNumber, ct)
            ?? throw new NotFoundException($"Ticket '{ticketNumber}' not found.");
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<byte[]> GeneratePdfAsync(string ticketNumber, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetByTicketNumberAsync(ticketNumber, ct)
            ?? throw new NotFoundException($"Ticket '{ticketNumber}' not found.");

        var qrBytes = await _qrCode.GenerateAsync(ticket.QrCodeData, ct);
        var model = BuildTicketPdfModel(ticket, qrBytes);
        return await _pdf.GenerateTicketPdfAsync(model, ct);
    }

    public async Task<byte[]> GenerateBoardingPassAsync(string ticketNumber, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetByTicketNumberAsync(ticketNumber, ct)
            ?? throw new NotFoundException($"Ticket '{ticketNumber}' not found.");

        var qrBytes = await _qrCode.GenerateAsync(ticket.QrCodeData, ct);
        var model = new Common.Interfaces.BoardingPassModel
        {
            TicketNumber = ticket.TicketNumber,
            BookingReference = ticket.Booking.BookingReference,
            PassengerName = ticket.BookingPassenger.FullName,
            FlightNumber = ticket.BookingSegment.Flight.FlightNumber,
            AirlineName = ticket.BookingSegment.Flight.Airline.Name,
            OriginIata = ticket.BookingSegment.Flight.Route.OriginAirport.IataCode,
            OriginCity = ticket.BookingSegment.Flight.Route.OriginAirport.City,
            DestinationIata = ticket.BookingSegment.Flight.Route.DestinationAirport.IataCode,
            DestinationCity = ticket.BookingSegment.Flight.Route.DestinationAirport.City,
            DepartureTime = ticket.BookingSegment.Flight.DepartureTime,
            ArrivalTime = ticket.BookingSegment.Flight.ArrivalTime,
            SeatClass = ticket.BookingSegment.SeatClass.ToString(),
            SeatNumber = ticket.BookingPassenger.Seat?.SeatNumber,
            Gate = ticket.BookingSegment.Flight.GateNumber ?? "TBD",
            Terminal = ticket.BookingSegment.Flight.Route.OriginAirport.Terminal ?? "TBD",
            QrCodeBytes = qrBytes,
            TotalAmount = ticket.BookingSegment.SegmentTotal
        };

        return await _pdf.GenerateBoardingPassPdfAsync(model, ct);
    }

    public async Task CheckInAsync(string ticketNumber, Guid userId, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetByTicketNumberAsync(ticketNumber, ct)
            ?? throw new NotFoundException($"Ticket '{ticketNumber}' not found.");

        if (ticket.Booking.UserId != userId) throw new ForbiddenAccessException();
        if (ticket.IsCheckedIn) throw new ConflictException("Already checked in.");

        var flight = ticket.BookingSegment.Flight;
        if (flight.DepartureTime <= DateTime.UtcNow.AddMinutes(30))
            throw new ConflictException("Check-in is closed for this flight.");

        ticket.IsCheckedIn = true;
        ticket.CheckedInAt = DateTime.UtcNow;
        _uow.Tickets.Update(ticket);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task GenerateAndPersistTicketsAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        var tickets = new List<Ticket>();
        foreach (var segment in booking.BookingSegments)
        {
            foreach (var passenger in booking.BookingPassengers)
            {
                var qrPayload = _qrCode.BuildTicketQrPayload(
                    string.Empty,
                    booking.BookingReference,
                    segment.Flight.FlightNumber,
                    segment.Flight.Route.OriginAirport.IataCode,
                    segment.Flight.Route.DestinationAirport.IataCode,
                    passenger.Seat?.SeatNumber ?? "TBD",
                    passenger.FullName,
                    segment.SeatClass.ToString(),
                    segment.Flight.DepartureTime);

                var ticketNumber = await GenerateUniqueTicketNumberAsync(ct);
                var finalQrPayload = _qrCode.BuildTicketQrPayload(
                    ticketNumber, booking.BookingReference,
                    segment.Flight.FlightNumber,
                    segment.Flight.Route.OriginAirport.IataCode,
                    segment.Flight.Route.DestinationAirport.IataCode,
                    passenger.Seat?.SeatNumber ?? "TBD",
                    passenger.FullName, segment.SeatClass.ToString(),
                    segment.Flight.DepartureTime);

                tickets.Add(new Ticket
                {
                    BookingId = bookingId,
                    BookingPassengerId = passenger.Id,
                    TicketNumber = ticketNumber,
                    BookingSegmentId = segment.Id,
                    QrCodeData = finalQrPayload,
                    IssuedAt = DateTime.UtcNow
                });
            }
        }

        await _uow.Tickets.AddRangeAsync(tickets, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Generated {Count} tickets for booking {BookingId}", tickets.Count, bookingId);
    }

    private async Task<string> GenerateUniqueTicketNumberAsync(CancellationToken ct)
    {
        string number;
        do
        {
            number = "TKT" + RandomNumberGenerator.GetInt32(10000000, 99999999).ToString();
        } while (await _uow.Tickets.AnyAsync(t => t.TicketNumber == number, ct));
        return number;
    }

    private static Common.Interfaces.TicketPdfModel BuildTicketPdfModel(Ticket ticket, byte[]? qrBytes)
    {
        return new Common.Interfaces.TicketPdfModel
        {
            TicketNumber = ticket.TicketNumber,
            BookingReference = ticket.Booking.BookingReference,
            PassengerName = ticket.BookingPassenger.FullName,
            FlightNumber = ticket.BookingSegment.Flight.FlightNumber,
            AirlineName = ticket.BookingSegment.Flight.Airline.Name,
            OriginIata = ticket.BookingSegment.Flight.Route.OriginAirport.IataCode,
            OriginCity = ticket.BookingSegment.Flight.Route.OriginAirport.City,
            DestinationIata = ticket.BookingSegment.Flight.Route.DestinationAirport.IataCode,
            DestinationCity = ticket.BookingSegment.Flight.Route.DestinationAirport.City,
            DepartureTime = ticket.BookingSegment.Flight.DepartureTime,
            ArrivalTime = ticket.BookingSegment.Flight.ArrivalTime,
            SeatClass = ticket.BookingSegment.SeatClass.ToString(),
            SeatNumber = ticket.BookingPassenger.Seat?.SeatNumber,
            TotalAmount = ticket.BookingSegment.SegmentTotal,
            QrCodeBytes = qrBytes
        };
    }
}
