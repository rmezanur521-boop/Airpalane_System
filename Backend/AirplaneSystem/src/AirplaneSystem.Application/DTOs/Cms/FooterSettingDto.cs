namespace AirplaneSystem.Application.DTOs.Cms;

public class FooterSettingDto
{
    public Guid Id { get; set; }
    public string? About { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? Youtube { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public string? Copyright { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateFooterSettingDto
{
    public string? About { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? Youtube { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public string? Copyright { get; set; }
}