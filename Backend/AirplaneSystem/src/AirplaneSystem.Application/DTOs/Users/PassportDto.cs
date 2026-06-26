namespace AirplaneSystem.Application.DTOs.Users;

public class PassportDto
{
    public string PassportNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public DateOnly IssuedDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
}
