using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Settings;

public class AdminSetting : BaseEntity
{
    // Company Info
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string? FaviconPath { get; set; }
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;

    // SMTP Settings
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpFromEmail { get; set; }

    // Footer
    public string? FooterText { get; set; }
}