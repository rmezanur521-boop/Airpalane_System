using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Cms;

public class SmtpSettings : BaseEntity
{
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpFromEmail { get; set; }
}