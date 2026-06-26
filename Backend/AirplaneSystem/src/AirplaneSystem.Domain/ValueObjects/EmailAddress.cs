using AirplaneSystem.Domain.Common;
using System.Text.RegularExpressions;

namespace AirplaneSystem.Domain.ValueObjects;

public class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress() { Value = string.Empty; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Email cannot be empty.", nameof(value));
        if (!EmailRegex.IsMatch(value)) throw new ArgumentException("Invalid email format.", nameof(value));
        Value = value.ToLowerInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
