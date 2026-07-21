using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class PaymentGatewaySettingService : IPaymentGatewaySettingService
{
    private readonly IUnitOfWork _uow;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<PaymentGatewaySettingService> _logger;

    private static readonly string[] SupportedProviders = { "Stripe", "Bkash", "Nagad" };

    public PaymentGatewaySettingService(IUnitOfWork uow, IEncryptionService encryption,
        ILogger<PaymentGatewaySettingService> logger)
    {
        _uow = uow;
        _encryption = encryption;
        _logger = logger;
    }

    public async Task<List<PaymentGatewaySettingDto>> GetAllAsync(CancellationToken ct = default)
    {
        var settings = await _uow.PaymentGatewaySettings.GetAllAsync(ct);
        var existingProviders = settings.Select(s => s.Provider).ToHashSet();

        // যেসব provider এখনো DB-তে নেই, সেগুলো empty state হিসেবে দেখাও যাতে admin UI-তে ফাঁকা কার্ড দেখা যায়
        var result = settings.Select(MapToDto).ToList();
        foreach (var provider in SupportedProviders.Except(existingProviders))
        {
            result.Add(new PaymentGatewaySettingDto
            {
                Provider = provider,
                IsEnabled = false,
                HasSecretKey = false,
                HasWebhookSecret = false
            });
        }
        return result;
    }

    public async Task<PaymentGatewaySettingDto> UpdateAsync(
    string provider, UpdatePaymentGatewaySettingRequest request, Guid? updatedBy, CancellationToken ct = default)
    {
        if (!SupportedProviders.Contains(provider))
            throw new AirplaneSystem.Application.Exceptions.ValidationException(
                "Provider", $"Unsupported provider '{provider}'.");

        var setting = await _uow.PaymentGatewaySettings.GetByProviderAsync(provider, ct);
        var isNew = setting == null;

        if (isNew)
        {
            setting = new PaymentGatewaySetting { Provider = provider };
        }

        setting.IsEnabled = request.IsEnabled;
        setting.PublicKey = request.PublicKey;

        if (!string.IsNullOrWhiteSpace(request.SecretKey))
            setting.SecretKey = _encryption.Encrypt(request.SecretKey);

        if (!string.IsNullOrWhiteSpace(request.WebhookSecret))
            setting.WebhookSecret = _encryption.Encrypt(request.WebhookSecret);

        if (request.ExtraConfigJson != null)
            setting.ExtraConfigJson = request.ExtraConfigJson;

        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = updatedBy;

        if (isNew)
        {
            await _uow.PaymentGatewaySettings.AddAsync(setting, ct);   // ✅ শুধু নতুনের জন্য Add
        }
        else
        {
            _uow.PaymentGatewaySettings.Update(setting);               // ✅ শুধু পুরনোর জন্য Update
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Payment gateway settings updated for {Provider} by {UserId}", provider, updatedBy);

        return MapToDto(setting);
    }
    public async Task<DecryptedGatewaySetting?> GetDecryptedByProviderAsync(string provider, CancellationToken ct = default)
    {
        var setting = await _uow.PaymentGatewaySettings.GetByProviderAsync(provider, ct);
        if (setting == null) return null;

        return new DecryptedGatewaySetting
        {
            IsEnabled = setting.IsEnabled,
            PublicKey = setting.PublicKey,
            SecretKey = string.IsNullOrEmpty(setting.SecretKey) ? null
                : (_encryption.IsEncrypted(setting.SecretKey) ? _encryption.Decrypt(setting.SecretKey) : setting.SecretKey),
            WebhookSecret = string.IsNullOrEmpty(setting.WebhookSecret) ? null
                : (_encryption.IsEncrypted(setting.WebhookSecret) ? _encryption.Decrypt(setting.WebhookSecret) : setting.WebhookSecret),
            ExtraConfigJson = setting.ExtraConfigJson
        };
    }

    public async Task<PublicPaymentConfigDto> GetPublicConfigAsync(CancellationToken ct = default)
    {
        var settings = await _uow.PaymentGatewaySettings.GetAllAsync(ct);
        var stripe = settings.FirstOrDefault(s => s.Provider == "Stripe");
        var bkash = settings.FirstOrDefault(s => s.Provider == "Bkash");
        var nagad = settings.FirstOrDefault(s => s.Provider == "Nagad");

        return new PublicPaymentConfigDto
        {
            StripeEnabled = stripe?.IsEnabled ?? false,
            StripePublicKey = stripe?.IsEnabled == true ? stripe.PublicKey : null,
            BkashEnabled = bkash?.IsEnabled ?? false,
            NagadEnabled = nagad?.IsEnabled ?? false
        };
    }

    private static PaymentGatewaySettingDto MapToDto(PaymentGatewaySetting s) => new()
    {
        Id = s.Id,
        Provider = s.Provider,
        IsEnabled = s.IsEnabled,
        PublicKey = s.PublicKey,
        SecretKeyMasked = MaskSecret(s.SecretKey),
        HasSecretKey = !string.IsNullOrEmpty(s.SecretKey),
        HasWebhookSecret = !string.IsNullOrEmpty(s.WebhookSecret)
    };

    private static string? MaskSecret(string? encrypted)
    {
        if (string.IsNullOrEmpty(encrypted)) return null;
        return "••••••••••••"; // চাইলে length অনুযায়ী কিছুটা ডিটেইল রাখতে পারো, কিন্তু raw decrypt করে দেখানো ঠিক না
    }
}