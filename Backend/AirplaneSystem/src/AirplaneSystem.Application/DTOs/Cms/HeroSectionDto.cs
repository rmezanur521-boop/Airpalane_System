using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Cms;

public class HeroSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? BackgroundImage { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public bool SearchBoxEnabled { get; set; }
    public double OverlayOpacity { get; set; }
    public ContentStatus Status { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}