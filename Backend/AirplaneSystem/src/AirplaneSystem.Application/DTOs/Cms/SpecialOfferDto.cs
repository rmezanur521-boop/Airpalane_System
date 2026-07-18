using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Cms;

public class SpecialOfferDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OfferImage { get; set; }
    public decimal Price { get; set; }
    public string? PromoCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int Priority { get; set; }
    public ContentStatus Status { get; set; }
    public bool Featured { get; set; }
}

public class CreateSpecialOfferDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? PromoCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int Priority { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Active;
    public bool Featured { get; set; }
}

public class UpdateSpecialOfferDto : CreateSpecialOfferDto { }