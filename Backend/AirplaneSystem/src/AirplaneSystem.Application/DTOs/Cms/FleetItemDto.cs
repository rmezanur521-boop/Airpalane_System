using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Cms;

public class FleetItemDto
{
    public Guid Id { get; set; }
    public string AircraftName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Image { get; set; }
    public int SeatCapacity { get; set; }
    public string? Range { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; }
}

public class CreateFleetItemDto
{
    public string AircraftName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public int SeatCapacity { get; set; }
    public string? Range { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
}

public class UpdateFleetItemDto : CreateFleetItemDto { }