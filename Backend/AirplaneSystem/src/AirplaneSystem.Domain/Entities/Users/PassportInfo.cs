using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Users;

public class PassportInfo : BaseEntity
{
    public Guid UserId { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public DateOnly IssuedDate { get; set; }
    public DateOnly ExpiryDate { get; set; }

    public User User { get; set; } = null!;
}
