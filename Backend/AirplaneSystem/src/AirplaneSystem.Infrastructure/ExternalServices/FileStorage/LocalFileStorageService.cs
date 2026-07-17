using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Infrastructure.ExternalServices.FileStorage;

/// <summary>
/// Stores uploaded files on local disk under {WebRoot}/uploads/{subFolder}/ and
/// serves them back via the static-files middleware at "/uploads/...".
/// The root path is supplied by the host (API project) so this project does not
/// need a dependency on IWebHostEnvironment.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".ico", ".svg" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly string _webRootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(string webRootPath, ILogger<LocalFileStorageService> logger)
    {
        _webRootPath = webRootPath;
        _logger = logger;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("file", "No file was uploaded.");

        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("file", "File size must not exceed 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ValidationException("file", "Only image files (jpg, jpeg, png, webp, gif, ico, svg) are allowed.");

        var folderPath = Path.Combine(_webRootPath, "uploads", subFolder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folderPath, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relativeUrl = $"/uploads/{subFolder}/{fileName}".Replace('\\', '/');
        _logger.LogInformation("Stored uploaded file at {Path}", relativeUrl);
        return relativeUrl;
    }

    public Task<byte[]?> ReadAsync(string? relativeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return Task.FromResult<byte[]?>(null);

        try
        {
            var trimmed = relativeUrl.TrimStart('/');
            var fullPath = Path.Combine(_webRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath)
                ? Task.FromResult<byte[]?>(File.ReadAllBytes(fullPath))
                : Task.FromResult<byte[]?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read stored file {RelativeUrl}", relativeUrl);
            return Task.FromResult<byte[]?>(null);
        }
    }

    public Task DeleteAsync(string? relativeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return Task.CompletedTask;

        try
        {
            var trimmed = relativeUrl.TrimStart('/');
            var fullPath = Path.Combine(_webRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            // Non-fatal: an orphaned file on disk should never break the request.
            _logger.LogWarning(ex, "Failed to delete stored file {RelativeUrl}", relativeUrl);
        }

        return Task.CompletedTask;
    }
}