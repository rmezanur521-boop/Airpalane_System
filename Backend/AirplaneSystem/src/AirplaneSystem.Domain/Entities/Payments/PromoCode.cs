using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Payments;

public class PromoCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public int MaxUses { get; set; }
    public int TimesUsed { get; set; } = 0;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public decimal MinimumAmount { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsValid => IsActive
        && TimesUsed < MaxUses
        && DateTime.UtcNow >= ValidFrom
        && DateTime.UtcNow <= ValidTo;

    public decimal CalculateDiscount(decimal cartTotal)
    {
        if (!IsValid || cartTotal < MinimumAmount) return 0;
        if (DiscountPercentage.HasValue)
            return Math.Round(cartTotal * (DiscountPercentage.Value / 100), 2);
        return DiscountAmount ?? 0;
    }
}
