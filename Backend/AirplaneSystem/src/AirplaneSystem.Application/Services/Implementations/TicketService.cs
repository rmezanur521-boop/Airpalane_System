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
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IUnitOfWork uow, IMapper mapper, IPdfService pdf,
        IQrCodeService qrCode, IFileStorageService fileStorage, ILogger<TicketService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _pdf = pdf;
        _qrCode = qrCode;
        _fileStorage = fileStorage;
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
        var model = await BuildTicketPdfModelAsync(ticket, qrBytes, ct);
        return await _pdf.GenerateTicketPdfAsync(model, ct);
    }

    public async Task<byte[]> GenerateBoardingPassAsync(string ticketNumber, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetByTicketNumberAsync(ticketNumber, ct)
            ?? throw new NotFoundException($"Ticket '{ticketNumber}' not found.");

        var qrBytes = await _qrCode.GenerateAsync(ticket.QrCodeData, ct);
        var flight = ticket.BookingSegment.Flight;
        var passenger = ticket.BookingPassenger;
        var origin = flight.Route?.OriginAirport;
        var destination = flight.Route?.DestinationAirport;
        var airline = flight.Airline;
        var logoBytes = await _fileStorage.ReadAsync(airline?.LogoUrl ?? string.Empty, ct);

        var baggage = ticket.BookingSegment.SeatClass switch
        {
            Domain.Enums.SeatClass.First => "2x 32kg Checked + 2x 10kg Cabin",
            Domain.Enums.SeatClass.Business => "2x 32kg Checked + 1x 10kg Cabin",
            _ => "1x 23kg Checked + 1x 7kg Cabin"
        };

        var model = new Common.Interfaces.BoardingPassModel
        {
            TicketNumber = ticket.TicketNumber,
            BookingReference = ticket.Booking.BookingReference,
            BookingDate = ticket.Booking.CreatedAt,
            PassengerName = passenger.FullName,
            FlightNumber = flight.FlightNumber,
            AirlineName = airline?.Name ?? "AirSystem International",
            AirlineIata = airline?.IataCode ?? "AS",
            AirlineLogoBytes = logoBytes,
            OriginIata = origin?.IataCode ?? "DAC",
            OriginCity = origin?.City ?? "Dhaka",
            OriginAirportName = origin?.Name ?? "Hazrat Shahjalal Intl",
            OriginCountry = origin?.Country ?? "Bangladesh",
            OriginTerminal = origin?.Terminal ?? "T1",
            DestinationIata = destination?.IataCode ?? "DXB",
            DestinationCity = destination?.City ?? "Dubai",
            DestinationAirportName = destination?.Name ?? "Dubai International",
            DestinationCountry = destination?.Country ?? "United Arab Emirates",
            DestinationTerminal = destination?.Terminal ?? "T3",
            AircraftModel = flight.Aircraft?.Model ?? "Boeing 777-300ER",
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            SeatClass = ticket.BookingSegment.SeatClass.ToString(),
            SeatNumber = passenger.Seat?.SeatNumber ?? "TBD",
            PassportNumber = passenger.PassportNumber,
            PassportCountry = passenger.PassportCountry,
            PassengerType = passenger.PassengerType.ToString(),
            MealPreference = passenger.MealPreference,
            BaggageAllowance = baggage,
            Gate = flight.GateNumber ?? "A12",
            Terminal = origin?.Terminal ?? "T1",
            BoardingGroup = ticket.BookingSegment.SeatClass == Domain.Enums.SeatClass.First ? "ZONE 1" 
                : (ticket.BookingSegment.SeatClass == Domain.Enums.SeatClass.Business ? "ZONE 2" : "ZONE 3"),
            BoardingTime = flight.DepartureTime.AddMinutes(-45),
            QrCodeBytes = qrBytes,
            TotalAmount = ticket.BookingSegment.SegmentTotal,
            PaymentStatus = ticket.Booking.Payment?.Status.ToString() ?? "Paid"
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

    private async Task<Common.Interfaces.TicketPdfModel> BuildTicketPdfModelAsync(
        Ticket ticket, byte[]? qrBytes, CancellationToken ct)
    {
        var airline = ticket.BookingSegment.Flight.Airline;
        var logoBytes = await _fileStorage.ReadAsync(airline?.LogoUrl ?? string.Empty, ct);
        var flight = ticket.BookingSegment.Flight;
        var passenger = ticket.BookingPassenger;
        var booking = ticket.Booking;
        var segment = ticket.BookingSegment;
        var origin = flight.Route?.OriginAirport;
        var destination = flight.Route?.DestinationAirport;

        var baggage = segment.SeatClass switch
        {
            Domain.Enums.SeatClass.First => "2x 32kg Checked + 2x 10kg Cabin",
            Domain.Enums.SeatClass.Business => "2x 32kg Checked + 1x 10kg Cabin",
            _ => "1x 23kg Checked + 1x 7kg Cabin"
        };

        var baseFare = segment.BaseFare > 0 ? segment.BaseFare : Math.Round(segment.SegmentTotal * 0.85m, 2);
        var taxesFees = (segment.Taxes + segment.Fees) > 0 ? (segment.Taxes + segment.Fees) : Math.Round(segment.SegmentTotal - baseFare, 2);

        return new Common.Interfaces.TicketPdfModel
        {
            TicketNumber = ticket.TicketNumber,
            BookingReference = booking.BookingReference,
            BookingDate = booking.CreatedAt,
            PassengerName = passenger.FullName,
            FlightNumber = flight.FlightNumber,
            AirlineName = airline?.Name ?? "AirSystem International",
            AirlineIata = airline?.IataCode ?? "AS",
            AirlineLogoBytes = logoBytes,
            OriginIata = origin?.IataCode ?? "DAC",
            OriginCity = origin?.City ?? "Dhaka",
            OriginAirportName = origin?.Name ?? "Hazrat Shahjalal Intl",
            OriginCountry = origin?.Country ?? "Bangladesh",
            OriginTerminal = origin?.Terminal ?? "T1",
            DestinationIata = destination?.IataCode ?? "DXB",
            DestinationCity = destination?.City ?? "Dubai",
            DestinationAirportName = destination?.Name ?? "Dubai International",
            DestinationCountry = destination?.Country ?? "United Arab Emirates",
            DestinationTerminal = destination?.Terminal ?? "T3",
            AircraftModel = flight.Aircraft?.Model ?? "Boeing 777-300ER",
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            SeatClass = segment.SeatClass.ToString(),
            SeatNumber = passenger.Seat?.SeatNumber,
            PassportNumber = passenger.PassportNumber,
            PassportCountry = passenger.PassportCountry,
            PassengerType = passenger.PassengerType.ToString(),
            MealPreference = passenger.MealPreference,
            BaggageAllowance = baggage,
            BaseFare = baseFare,
            TaxesAndFees = taxesFees,
            DiscountAmount = booking.DiscountAmount,
            TotalAmount = segment.SegmentTotal,
            PaymentStatus = booking.Payment?.Status.ToString() ?? "Paid",
            PaymentMethod = booking.Payment?.Method.ToString() ?? "Stripe Credit Card",
            TransactionReference = booking.Payment?.ReferenceNumber ?? booking.Payment?.StripePaymentIntentId,
            Gate = flight.GateNumber ?? "A12",
            QrCodeBytes = qrBytes
        };
    }
}