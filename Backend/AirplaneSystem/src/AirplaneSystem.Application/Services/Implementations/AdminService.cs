using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.DTOs.Users;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Audit;
using AirplaneSystem.Domain.Entities.Users;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(IUnitOfWork uow, IMapper mapper, ILogger<AdminService> logger, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<DashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var allBookings = await _uow.Bookings.GetAllAsync(ct);
        var periodBookings = allBookings.Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate).ToList();

        var allFlights = await _uow.Flights.GetAllAsync(ct);
        var allUsers = await _uow.Users.GetAllAsync(ct);

        var revenueByDay = periodBookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .GroupBy(b => DateOnly.FromDateTime(b.CreatedAt))
            .Select(g => new RevenueByDay { Date = g.Key, Revenue = g.Sum(b => b.TotalAmount), Bookings = g.Count() })
            .OrderBy(r => r.Date)
            .ToList();

        return new DashboardDto
        {
            TotalBookings = periodBookings.Count,
            ConfirmedBookings = periodBookings.Count(b => b.Status == BookingStatus.Confirmed),
            PendingBookings = periodBookings.Count(b => b.Status == BookingStatus.PendingPayment),
            CancelledBookings = periodBookings.Count(b => b.Status == BookingStatus.Cancelled),
            TotalRevenue = periodBookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalAmount),
            RevenueToday = periodBookings.Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt.Date == DateTime.UtcNow.Date).Sum(b => b.TotalAmount),
            TotalUsers = allUsers.Count,
            ActiveFlights = allFlights.Count(f => f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding),
            DelayedFlights = allFlights.Count(f => f.Status == FlightStatus.Delayed),
            CancelledFlights = allFlights.Count(f => f.Status == FlightStatus.Cancelled),
            RevenueByLastSevenDays = revenueByDay.TakeLast(7).ToList()
        };
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var bookings = (await _uow.Bookings.GetAllAsync(ct))
            .Where(b => b.Status == BookingStatus.Confirmed && b.ConfirmedAt >= from && b.ConfirmedAt <= to)
            .ToList();

        var totalRevenue = bookings.Sum(b => b.TotalAmount);
        var ticketCount = bookings.Sum(b => b.BookingPassengers.Count);

        return new RevenueReportDto
        {
            TotalRevenue = totalRevenue,
            TotalTicketsSold = ticketCount,
            AverageTicketPrice = ticketCount > 0 ? Math.Round(totalRevenue / ticketCount, 2) : 0
        };
    }

    public async Task<BookingReportDto> GetBookingReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var bookings = (await _uow.Bookings.GetAllAsync(ct))
            .Where(b => b.CreatedAt >= from && b.CreatedAt <= to)
            .ToList();

        var total = bookings.Count;
        var confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed);
        var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);

        return new BookingReportDto
        {
            TotalBookings = total,
            Confirmed = confirmed,
            Cancelled = cancelled,
            Expired = bookings.Count(b => b.Status == BookingStatus.Expired),
            Refunded = bookings.Count(b => b.Status == BookingStatus.Refunded),
            CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 2) : 0
        };
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        var total = await _uow.AuditLogs.CountAsync(query, ct);
        var items = await _uow.AuditLogs.GetPagedAsync(query, ct);
        var dtos = items.Select(a => _mapper.Map<AuditLogDto>(a)).ToList();
        return PagedResult<AuditLogDto>.Create(dtos, total, query.PageNumber, query.PageSize);
    }

    public async Task<UserDto> CreateAgentAsync(CreateAgentRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.EmailExistsAsync(request.Email, ct))
            throw new ConflictException($"User with email '{request.Email}' already exists.");

        var tempPassword = Guid.NewGuid().ToString("N")[..12] + "A1!";
        var agent = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(tempPassword),
            Role = UserRole.Agent,
            IsEmailVerified = true,
            IsActive = true,
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            Nationality = "US"
        };

        await _uow.Users.AddAsync(agent, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Agent account created: {Email} by admin", agent.Email);
        return _mapper.Map<UserDto>(agent);
    }
}
