namespace AirplaneSystem.Application.DTOs.Cms;

public class NavbarSettingDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool ShowLogin { get; set; }
    public bool ShowSignup { get; set; }
    public bool ShowLanguage { get; set; }
    public bool ShowCurrency { get; set; }
    public bool AnnouncementEnabled { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateNavbarSettingDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool ShowLogin { get; set; } = true;
    public bool ShowSignup { get; set; } = true;
    public bool ShowLanguage { get; set; }
    public bool ShowCurrency { get; set; }
    public bool AnnouncementEnabled { get; set; }
}