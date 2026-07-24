namespace AirplaneSystem.Application.DTOs.Cms;

public class SmtpSettingDto
{
    public Guid Id { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpFromEmail { get; set; }
    public bool IsPasswordSet { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateSmtpSettingDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string? SmtpPassword { get; set; }
    public string SmtpFromName { get; set; } = string.Empty;
    public string SmtpFromEmail { get; set; } = string.Empty;
}