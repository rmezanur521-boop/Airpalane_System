using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Cms;

public class HeroSection : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? BackgroundImage { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public bool SearchBoxEnabled { get; set; } = true;
    public double OverlayOpacity { get; set; } = 0.4;
    public ContentStatus Status { get; set; } = ContentStatus.Active;
    public int DisplayOrder { get; set; }
}