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

    // Enriched fields for International Standard E-Ticket & Boarding Pass
    public string? OriginAirportName { get; set; }
    public string? DestinationAirportName { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public string? OriginTerminal { get; set; }
    public string? DestinationTerminal { get; set; }
    public string? AircraftModel { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportCountry { get; set; }
    public string? PassengerType { get; set; }
    public string? MealPreference { get; set; }
    public string? BaggageAllowance { get; set; }
    public decimal BaseFare { get; set; }
    public decimal TaxesAndFees { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? Gate { get; set; }
}

public class BoardingPassModel : TicketPdfModel
{
    public string Terminal { get; set; } = string.Empty;
    public string? BoardingGroup { get; set; }
    public DateTime BoardingTime { get; set; }
}
