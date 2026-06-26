using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.ValueObjects;

public class IataCode : ValueObject
{
    public string Value { get; }

    private IataCode() { Value = string.Empty; }

    public IataCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("IATA code cannot be empty.", nameof(value));
        if (value.Length < 2 || value.Length > 3) throw new ArgumentException("IATA code must be 2-3 characters.", nameof(value));
        Value = value.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(IataCode code) => code.Value;
}
