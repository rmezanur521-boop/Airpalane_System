using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.DTOs.Admin;

public class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
}