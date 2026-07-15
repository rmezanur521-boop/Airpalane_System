namespace AirplaneSystem.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateTicketPdfAsync(TicketPdfModel model, CancellationToken ct = default);
    Task<byte[]> GenerateBoardingPassPdfAsync(BoardingPassModel model, CancellationToken ct = default);
}

public class TicketPdfModel
{
    public string TicketNumber { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string AirlineIata { get; set; } = string.Empty;
    public byte[]? AirlineLogoBytes { get; set; }
    public string OriginIata { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string SeatClass { get; set; } = string.Empty;
    public string? SeatNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
    public byte[]? QrCodeBytes { get; set; }
}

public class BoardingPassModel : TicketPdfModel
{
    public string Gate { get; set; } = string.Empty;
    public string Terminal { get; set; } = string.Empty;
}