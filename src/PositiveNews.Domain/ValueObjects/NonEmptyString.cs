using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// Guarantees a trimmed, non-empty string value.
/// </summary>
public sealed record NonEmptyString
{
    /// <summary>Trimmed content.</summary>
    public string Value { get; }

    private NonEmptyString(string value) => Value = value;

    /// <summary>
    /// Validates non-whitespace input and returns a trimmed wrapper.
    /// </summary>
    public static NonEmptyString Create(string? value, string fieldName = "value")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"'{fieldName}' cannot be empty.");
        return new NonEmptyString(value.Trim());
    }

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(NonEmptyString v) => v.Value;

    /// <inheritdoc />
    public override string ToString() => Value;
}
