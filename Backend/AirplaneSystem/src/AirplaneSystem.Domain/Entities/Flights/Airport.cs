using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Airport : BaseEntity
{
    public string IataCode { get; set; } = string.Empty;
    public string IcaoCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string? Terminal { get; set; }
    public bool IsActive { get; set; } = true;
}
