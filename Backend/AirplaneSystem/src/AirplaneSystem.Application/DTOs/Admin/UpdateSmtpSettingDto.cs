namespace AirplaneSystem.Application.DTOs.Admin;

public class UpdateSmtpSettingDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Optional — যদি Admin Password Field ফাঁকা রেখে শুধু Host/Port Update করতে চান,
    /// তাহলে বিদ্যমান Encrypted Password অপরিবর্তিত থাকবে (Overwrite হবে না)।
    /// </summary>
    public string? SmtpPassword { get; set; }

    public string SmtpFromName { get; set; } = string.Empty;
    public string SmtpFromEmail { get; set; } = string.Empty;
}