namespace AirplaneSystem.Application.DTOs.Admin;

public class DashboardDto
{
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveFlights { get; set; }
    public int DelayedFlights { get; set; }
    public int CancelledFlights { get; set; }
    public List<RevenueByDay> RevenueByLastSevenDays { get; set; } = new();
}

public class RevenueByDay
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int Bookings { get; set; }
}

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal AverageTicketPrice { get; set; }
    public int TotalTicketsSold { get; set; }
    public List<RevenueByRoute> TopRoutes { get; set; } = new();
    public List<RevenueByAirline> RevenueByAirline { get; set; } = new();
}

public class RevenueByRoute
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenueByAirline
{
    public string AirlineName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class BookingReportDto
{
    public int TotalBookings { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int Expired { get; set; }
    public int Refunded { get; set; }
    public decimal CancellationRate { get; set; }
}

public class AuditLogDto
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class CreateAgentRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class AuditLogQuery : Common.Models.PaginationQuery
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
