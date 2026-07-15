using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Bookings;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Booking;
using AirplaneSystem.Domain.Enums;
using AirplaneSystem.Domain.Events;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly INotificationService _notification;
    private readonly IDateTimeService _dateTime;

    public BookingService(IUnitOfWork uow, IMapper mapper, ILogger<BookingService> logger,
        INotificationService notification, IDateTimeService dateTime)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _notification = notification;
        _dateTime = dateTime;
    }

    public async Task<BookingDto> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default)
    {
        //await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                ?? throw new NotFoundException("User", userId);

            decimal totalAmount = 0;
            decimal discountAmount = 0;
            var segments = new List<BookingSegment>();

            foreach (var segReq in request.Segments.OrderBy(s => request.Segments.IndexOf(s)))
            {
                var flight = await _uow.Flights.GetByIdAsync(segReq.FlightId, ct)
                    ?? throw new NotFoundException("Flight", segReq.FlightId);

                if (flight.Status == FlightStatus.Cancelled)
                    throw new ConflictException($"Flight {flight.FlightNumber} is cancelled.");

                var availableSeats = flight.GetAvailableSeatsForClass(segReq.SeatClass);
                if (availableSeats < request.Passengers.Count)
                    throw new ConflictException($"Not enough seats available in {segReq.SeatClass} class for flight {flight.FlightNumber}.");

                var baseFare = flight.GetBasePriceForClass(segReq.SeatClass) * request.Passengers.Count;
                var taxRate = flight.TaxPercentage / 100;
                var taxes = Math.Round(baseFare * taxRate, 2);
                var fees = flight.AirportFee * request.Passengers.Count;

                segments.Add(new BookingSegment
                {
                    FlightId = flight.Id,
                    SegmentOrder = segments.Count + 1,
                    SeatClass = segReq.SeatClass,
                    BaseFare = baseFare,
                    Taxes = taxes,
                    Fees = fees,
                    SegmentTotal = baseFare + taxes + fees
                });

                totalAmount += baseFare + taxes + fees;

                // Reduce available seats
                switch (segReq.SeatClass)
                {
                    case SeatClass.Economy: flight.AvailableEconomySeats -= request.Passengers.Count; break;
                    case SeatClass.Business: flight.AvailableBusinessSeats -= request.Passengers.Count; break;
                    case SeatClass.First: flight.AvailableFirstClassSeats -= request.Passengers.Count; break;
                }
                _uow.Flights.Update(flight);
            }

            // Apply promo code
            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                var promo = await _uow.PromoCodes.GetByCodeAsync(request.PromoCode.ToUpperInvariant(), ct);
                if (promo != null && promo.IsValid && totalAmount >= promo.MinimumAmount)
                {
                    discountAmount = promo.CalculateDiscount(totalAmount);
                    promo.TimesUsed++;
                    _uow.PromoCodes.Update(promo);
                }
            }

            var booking = new Booking
            {
                BookingReference = await GenerateUniqueReferenceAsync(ct),
                UserId = userId,
                Status = BookingStatus.PendingPayment,
                TripType = request.TripType,
                TotalAmount = totalAmount - discountAmount,
                DiscountAmount = discountAmount,
                HoldExpiresAt = _dateTime.UtcNow.AddMinutes(15),
                BookingSegments = segments,
                BookingPassengers = request.Passengers.Select(p => new BookingPassenger
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PassengerType = p.PassengerType,
                    DateOfBirth = p.DateOfBirth,
                    PassportNumber = p.PassportNumber,
                    PassportExpiry = p.PassportExpiry,
                    PassportCountry = p.PassportCountry,
                    SeatId = p.SeatId,
                    MealPreference = p.MealPreference,
                    SpecialAssistance = p.SpecialAssistance
                }).ToList()
            };

            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                var promo = await _uow.PromoCodes.GetByCodeAsync(request.PromoCode.ToUpperInvariant(), ct);
                if (promo != null) booking.PromoCodeId = promo.Id;
            }

            booking.AddDomainEvent(new BookingCreatedEvent(booking.Id, userId, booking.BookingReference));

            await _uow.Bookings.AddAsync(booking, ct);
            await _uow.SaveChangesAsync(ct);
            //await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Booking created: {Reference} for user {UserId}", booking.BookingReference, userId);

            var created = await _uow.Bookings.GetWithDetailsAsync(booking.Id, ct);
            return _mapper.Map<BookingDto>(created);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<BookingDto> GetByIdAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId)
            throw new ForbiddenAccessException("You do not have access to this booking.");

        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> GetByReferenceAsync(string reference, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetByReferenceAsync(reference, ct)
            ?? throw new NotFoundException($"Booking with reference '{reference}' not found.");
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<PagedResult<BookingDto>> GetUserBookingsAsync(Guid userId, PaginationQuery query, CancellationToken ct = default)
    {
        var bookings = await _uow.Bookings.GetUserBookingsWithDetailsAsync(userId, ct);
        var total = bookings.Count;
        var items = bookings
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => _mapper.Map<BookingDto>(b))
            .ToList();
        return PagedResult<BookingDto>.Create(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<PagedResult<BookingDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var paged = await _uow.Bookings.GetPagedAsync(query, ct);
        var items = paged.Items.Select(b => _mapper.Map<BookingDto>(b)).ToList();
        return PagedResult<BookingDto>.Create(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task CancelAsync(Guid userId, Guid bookingId, string reason, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId) throw new ForbiddenAccessException();
        if (booking.Status == BookingStatus.Cancelled) throw new ConflictException("Booking is already cancelled.");
        if (booking.Status == BookingStatus.Expired) throw new ConflictException("Cannot cancel an expired booking.");

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = _dateTime.UtcNow;
        booking.CancellationReason = reason;

        // Release seats
        foreach (var segment in booking.BookingSegments)
        {
            var flight = await _uow.Flights.GetByIdAsync(segment.FlightId, ct);
            if (flight != null)
            {
                var passengerCount = booking.BookingPassengers.Count;
                switch (segment.SeatClass)
                {
                    case SeatClass.Economy: flight.AvailableEconomySeats += passengerCount; break;
                    case SeatClass.Business: flight.AvailableBusinessSeats += passengerCount; break;
                    case SeatClass.First: flight.AvailableFirstClassSeats += passengerCount; break;
                }
                _uow.Flights.Update(flight);
            }
        }

        _uow.Bookings.Update(booking);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Booking {Reference} cancelled by user {UserId}: {Reason}", booking.BookingReference, userId, reason);
    }

    public async Task SelectSeatAsync(Guid bookingId, SelectSeatRequest request, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        var passenger = booking.BookingPassengers.FirstOrDefault(p => p.Id == request.PassengerId)
            ?? throw new NotFoundException("Passenger", request.PassengerId);

        // Verify the seat exists and is available
        var flightId = booking.BookingSegments.FirstOrDefault()?.FlightId
            ?? throw new NotFoundException("Booking has no flight segments.");
        var flight = await _uow.Flights.GetWithSeatsAsync(flightId, ct)
            ?? throw new NotFoundException("Flight", flightId);

        var seat = flight.Seats.FirstOrDefault(s => s.Id == request.SeatId)
            ?? throw new NotFoundException("Seat", request.SeatId);

        if (!seat.IsAvailable) throw new ConflictException("The selected seat is not available.");

        // Release old seat if any
        if (passenger.SeatId.HasValue)
        {
            var oldSeat = flight.Seats.FirstOrDefault(s => s.Id == passenger.SeatId.Value);
            if (oldSeat != null) { oldSeat.IsAvailable = true; }
        }

        seat.IsAvailable = false;
        passenger.SeatId = seat.Id;
        _uow.Bookings.Update(booking);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ExpireHeldBookingsAsync(CancellationToken ct = default)
    {
        var expiredBookings = await _uow.Bookings.GetExpiredHoldsAsync(ct);
        if (!expiredBookings.Any()) return;

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Expired;
            booking.CancellationReason = "Booking hold expired — payment not completed in time.";
            booking.AddDomainEvent(new BookingExpiredEvent(booking.Id, booking.BookingReference));

            // Release seats
            foreach (var segment in booking.BookingSegments)
            {
                var flight = await _uow.Flights.GetByIdAsync(segment.FlightId, ct);
                if (flight == null) continue;
                var count = booking.BookingPassengers.Count;
                switch (segment.SeatClass)
                {
                    case SeatClass.Economy: flight.AvailableEconomySeats += count; break;
                    case SeatClass.Business: flight.AvailableBusinessSeats += count; break;
                    case SeatClass.First: flight.AvailableFirstClassSeats += count; break;
                }
                _uow.Flights.Update(flight);
            }
            _uow.Bookings.Update(booking);
        }

        await _uow.SaveChangesAsync(ct);

        foreach (var booking in expiredBookings)
        {
            try { await _notification.SendBookingExpiryNoticeAsync(booking.Id, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send expiry notice for booking {Id}", booking.Id); }
        }

        _logger.LogInformation("Expired {Count} held bookings", expiredBookings.Count);
    }

    private async Task<string> GenerateUniqueReferenceAsync(CancellationToken ct)
    {
        string reference;
        do
        {
            reference = GenerateReference();
        } while (await _uow.Bookings.ReferenceExistsAsync(reference, ct));
        return reference;
    }

    private static string GenerateReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
