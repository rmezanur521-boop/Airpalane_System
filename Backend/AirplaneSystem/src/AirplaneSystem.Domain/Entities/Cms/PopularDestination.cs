using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Cms;

public class PopularDestination : BaseEntity
{
    public string DestinationName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public decimal StartingPrice { get; set; }
    public string? ButtonLink { get; set; }
    public bool Featured { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
}