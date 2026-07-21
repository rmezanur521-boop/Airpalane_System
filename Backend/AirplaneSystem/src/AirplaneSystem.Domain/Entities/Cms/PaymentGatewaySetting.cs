using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Cms;

public class PaymentGatewaySetting : BaseEntity
{
    public string Provider { get; set; } = string.Empty; // "Stripe", "Bkash", "Nagad"
    public bool IsEnabled { get; set; }

    public string? PublicKey { get; set; }      // Stripe publishable key / bKash app key — plaintext
    public string? SecretKey { get; set; }       // Encrypted at rest (Stripe secret / bKash app secret)
    public string? WebhookSecret { get; set; }   // Encrypted at rest

    public string? ExtraConfigJson { get; set; }
    public Guid? UpdatedBy { get; set; }
}