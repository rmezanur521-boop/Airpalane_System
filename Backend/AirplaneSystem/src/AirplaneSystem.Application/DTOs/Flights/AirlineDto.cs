namespace AirplaneSystem.Application.DTOs.Flights;

public class AirlineImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class AirlineDto
{
    public Guid Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public List<AirlineImageDto> Images { get; set; } = new();

    /// <summary>Convenience field for list/card views: the flagged primary image,
    /// falling back to the first gallery image if none is flagged.</summary>
    public string? PrimaryImageUrl =>
        Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? Images.FirstOrDefault()?.ImageUrl;
}