using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Aircraft : BaseEntity
{
    public Guid AirlineId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int EconomySeats { get; set; }
    public int BusinessSeats { get; set; }
    public int FirstClassSeats { get; set; }
    public bool IsActive { get; set; } = true;

    public Airline Airline { get; set; } = null!;
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
