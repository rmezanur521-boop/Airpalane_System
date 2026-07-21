using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.DTOs.Cms;
namespace AirplaneSystem.Application.Services.Interfaces;

public interface IPaymentGatewaySettingService
{
    Task<List<PaymentGatewaySettingDto>> GetAllAsync(CancellationToken ct = default);
    Task<PaymentGatewaySettingDto> UpdateAsync(string provider, UpdatePaymentGatewaySettingRequest request, Guid? updatedBy, CancellationToken ct = default);
    Task<DecryptedGatewaySetting?> GetDecryptedByProviderAsync(string provider, CancellationToken ct = default);

    Task<PublicPaymentConfigDto> GetPublicConfigAsync(CancellationToken ct = default);
   
}

public class DecryptedGatewaySetting
{
    public bool IsEnabled { get; set; }
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }       // decrypted
    public string? WebhookSecret { get; set; }   // decrypted
    public string? ExtraConfigJson { get; set; }
}