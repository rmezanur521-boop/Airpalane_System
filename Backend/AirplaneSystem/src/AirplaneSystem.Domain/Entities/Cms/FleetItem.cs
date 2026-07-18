using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Cms;

public class FleetItem : BaseEntity
{
    public string AircraftName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Image { get; set; }
    public int SeatCapacity { get; set; }
    public string? Range { get; set; }       // e.g. "12,000 km" — display-only text
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
}