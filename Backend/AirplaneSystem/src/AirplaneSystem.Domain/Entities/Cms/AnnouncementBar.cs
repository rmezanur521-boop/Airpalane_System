using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Cms;

public class AnnouncementBar : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
    public int Priority { get; set; }
}