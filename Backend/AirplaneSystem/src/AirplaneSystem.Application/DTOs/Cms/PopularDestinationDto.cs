using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Cms;

public class PopularDestinationDto
{
    public Guid Id { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public decimal StartingPrice { get; set; }
    public string? ButtonLink { get; set; }
    public bool Featured { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; }
}

public class CreatePopularDestinationDto
{
    public string DestinationName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal StartingPrice { get; set; }
    public string? ButtonLink { get; set; }
    public bool Featured { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
}

public class UpdatePopularDestinationDto : CreatePopularDestinationDto { }