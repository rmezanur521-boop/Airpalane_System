using AirplaneSystem.Domain.Common;

public class NavbarSetting : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? FaviconPath { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool ShowLogin { get; set; } = true;
    public bool ShowSignup { get; set; } = true;
    public bool ShowLanguage { get; set; }
    public bool ShowCurrency { get; set; }
    public bool AnnouncementEnabled { get; set; }
}