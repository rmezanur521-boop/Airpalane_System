using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class AdminSettingService : IAdminSettingService
{
    private const string LogoSubFolder = "company/logo";
    private const string FaviconSubFolder = "company/favicon";

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AdminSettingService> _logger;

    public AdminSettingService(
        IUnitOfWork uow,
        IMapper mapper,
        IEncryptionService encryptionService,
        IFileStorageService fileStorageService,
        ILogger<AdminSettingService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _encryptionService = encryptionService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<AdminSettingDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> UpdateGeneralSettingsAsync(
        UpdateAdminSettingDto dto, CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        settings.CompanyName = dto.CompanyName;
        settings.SupportEmail = dto.SupportEmail;
        settings.SupportPhone = dto.SupportPhone;
        settings.CompanyAddress = dto.CompanyAddress;
        settings.WebsiteUrl = dto.WebsiteUrl;
        settings.FooterText = dto.FooterText;

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Admin general settings updated.");
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> UpdateSmtpSettingsAsync(
        UpdateSmtpSettingDto dto, CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        settings.SmtpHost = dto.SmtpHost;
        settings.SmtpPort = dto.SmtpPort;
        settings.SmtpUsername = dto.SmtpUsername;
        settings.SmtpFromName = dto.SmtpFromName;
        settings.SmtpFromEmail = dto.SmtpFromEmail;

        // Password touched শুধু তখনই Overwrite করব যখন Admin নতুন কিছু দিয়েছেন।
        // ফাঁকা রাখলে বিদ্যমান Encrypted Value অপরিবর্তিত থাকবে।
        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
        {
            settings.SmtpPasswordEncrypted = _encryptionService.Encrypt(dto.SmtpPassword);
        }

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Admin SMTP settings updated.");
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> UploadLogoAsync(IFormFile file, CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        // পুরনো Logo থাকলে আগে সেটা মুছে ফেলা — Orphan File Accumulate হওয়া ঠেকাতে
        await _fileStorageService.DeleteAsync(settings.CompanyLogoPath, ct);

        var relativeUrl = await _fileStorageService.SaveAsync(file, LogoSubFolder, ct);
        settings.CompanyLogoPath = relativeUrl;

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Company logo uploaded: {Path}", relativeUrl);
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> DeleteLogoAsync(CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        await _fileStorageService.DeleteAsync(settings.CompanyLogoPath, ct);
        settings.CompanyLogoPath = null;

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Company logo removed.");
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> UploadFaviconAsync(IFormFile file, CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        await _fileStorageService.DeleteAsync(settings.FaviconPath, ct);

        var relativeUrl = await _fileStorageService.SaveAsync(file, FaviconSubFolder, ct);
        settings.FaviconPath = relativeUrl;

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Favicon uploaded: {Path}", relativeUrl);
        return _mapper.Map<AdminSettingDto>(settings);
    }

    public async Task<AdminSettingDto> DeleteFaviconAsync(CancellationToken ct = default)
    {
        var settings = await GetSingletonOrThrowAsync(ct);

        await _fileStorageService.DeleteAsync(settings.FaviconPath, ct);
        settings.FaviconPath = null;

        _uow.AdminSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Favicon removed.");
        return _mapper.Map<AdminSettingDto>(settings);
    }

    /// <summary>
    /// Fetches the singleton AdminSetting row. Throws if it's somehow missing
    /// (should never happen post-seed, but guards against a fresh/un-seeded DB).
    /// </summary>
    private async Task<AdminSetting> GetSingletonOrThrowAsync(CancellationToken ct)
    {
        var settings = await _uow.AdminSettings.GetSettingsAsync(ct);
        if (settings == null)
            throw new NotFoundException(nameof(AdminSetting), "settings");

        return settings;
    }
}