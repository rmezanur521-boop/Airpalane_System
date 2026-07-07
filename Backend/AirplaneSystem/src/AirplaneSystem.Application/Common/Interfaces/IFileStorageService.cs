using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Common.Interfaces;

/// <summary>
/// Abstraction over physical file storage for uploaded images (airline logos/gallery,
/// passenger profile pictures, etc). Kept provider-agnostic so the local-disk
/// implementation can later be swapped for cloud storage (Azure Blob/S3) without
/// touching callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file under the given sub-folder (e.g. "airlines/logos").
    /// Returns a public, relative URL (e.g. "/uploads/airlines/logos/xxxx.jpg").
    /// </summary>
    Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default);

    /// <summary>
    /// Deletes a previously-stored file given its public relative URL.
    /// Safe to call with null/unknown paths (no-op).
    /// </summary>
    Task DeleteAsync(string? relativeUrl, CancellationToken ct = default);
}