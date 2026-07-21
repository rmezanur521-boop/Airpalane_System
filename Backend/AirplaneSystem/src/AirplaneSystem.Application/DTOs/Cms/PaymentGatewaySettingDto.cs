namespace AirplaneSystem.Application.DTOs.Cms;

public class PaymentGatewaySettingDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? PublicKey { get; set; }
    public string? SecretKeyMasked { get; set; }    
    public bool HasSecretKey { get; set; }
    public bool HasWebhookSecret { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePaymentGatewaySettingRequest
{
    public bool IsEnabled { get; set; }
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? ExtraConfigJson { get; set; }
}


public class PublicPaymentConfigDto
{
    public bool StripeEnabled { get; set; }
    public string? StripePublicKey { get; set; }
    public bool BkashEnabled { get; set; }
    public bool NagadEnabled { get; set; }
}