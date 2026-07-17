namespace AirplaneSystem.Application.DTOs.Admin;

public class AdminSettingDto
{
    public Guid Id { get; set; }

    // Company Info
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;

    // SMTP (Password কখনো Return হয় না — শুধু Configured কি না বোঝানো হয়)
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public bool IsSmtpPasswordConfigured { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpFromEmail { get; set; }

    // Footer
    public string? FooterText { get; set; }

    public DateTime? UpdatedAt { get; set; }
}