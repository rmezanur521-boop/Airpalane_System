using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Flights;

public class AirlineImage : BaseEntity
{
    public Guid AirlineId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;

    public Airline Airline { get; set; } = null!;
}