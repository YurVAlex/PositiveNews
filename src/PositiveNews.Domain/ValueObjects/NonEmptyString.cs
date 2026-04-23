using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// Guarantees a trimmed, non-empty string value.
/// </summary>
public sealed record NonEmptyString
{
    public string Value { get; }

    private NonEmptyString(string value) => Value = value;

    public static NonEmptyString Create(string? value, string fieldName = "value")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"'{fieldName}' cannot be empty.");
        return new NonEmptyString(value.Trim());
    }

    public static implicit operator string(NonEmptyString v) => v.Value;

    public override string ToString() => Value;
}
