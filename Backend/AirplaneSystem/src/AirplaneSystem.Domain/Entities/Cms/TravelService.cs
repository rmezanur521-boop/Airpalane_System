using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Cms;

public class TravelService : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Image { get; set; }
    public string? ButtonText { get; set; }
    public string? RedirectUrl { get; set; }
    public bool IsExternal { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
}