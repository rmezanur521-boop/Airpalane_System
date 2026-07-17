namespace AirplaneSystem.Application.DTOs.Admin;

public class UpdateAdminSettingDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? FooterText { get; set; }
}