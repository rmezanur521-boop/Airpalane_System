using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Airline : BaseEntity
{
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public ICollection<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
    public ICollection<AirlineImage> Images { get; set; } = new List<AirlineImage>();
}
