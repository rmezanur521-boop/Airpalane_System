using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class SmtpSettingService : ISmtpSettingService
{
    private readonly IUnitOfWork _uow;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SmtpSettingService> _logger;

    public SmtpSettingService(
        IUnitOfWork uow,
        IEncryptionService encryptionService,
        ILogger<SmtpSettingService> logger)
    {
        _uow = uow;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<SmtpSettingDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await _uow.SmtpSettings.GetSingletonAsync(ct);
        return ToDto(settings);
    }

    public async Task<SmtpSettingDto> UpdateSmtpSettingsAsync(UpdateSmtpSettingDto dto, CancellationToken ct = default)
    {
        var settings = await _uow.SmtpSettings.GetSingletonAsync(ct);

        settings.SmtpHost = dto.SmtpHost;
        settings.SmtpPort = dto.SmtpPort;
        settings.SmtpUsername = dto.SmtpUsername;
        settings.SmtpFromName = dto.SmtpFromName;
        settings.SmtpFromEmail = dto.SmtpFromEmail;

        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
        {
            settings.SmtpPasswordEncrypted = _encryptionService.Encrypt(dto.SmtpPassword);
        }

        _uow.SmtpSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("SMTP settings updated via CMS.");
        return ToDto(settings);
    }

    private static SmtpSettingDto ToDto(AirplaneSystem.Domain.Entities.Cms.SmtpSettings settings) => new()
    {
        Id = settings.Id,
        SmtpHost = settings.SmtpHost,
        SmtpPort = settings.SmtpPort,
        SmtpUsername = settings.SmtpUsername,
        SmtpFromName = settings.SmtpFromName,
        SmtpFromEmail = settings.SmtpFromEmail,
        IsPasswordSet = !string.IsNullOrWhiteSpace(settings.SmtpPasswordEncrypted),
        UpdatedAt = settings.UpdatedAt
    };
}