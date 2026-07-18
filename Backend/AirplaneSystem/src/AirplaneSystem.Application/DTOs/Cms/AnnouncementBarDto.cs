using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Cms;

public class AnnouncementBarDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContentStatus Status { get; set; }
    public int Priority { get; set; }
}

public class CreateAnnouncementBarDto
{
    public string Title { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
    public int Priority { get; set; }
}

public class UpdateAnnouncementBarDto : CreateAnnouncementBarDto { }