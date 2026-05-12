using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A language code (e.g. "en", "en-US", "und") between 2 and 10 characters.
/// </summary>
public sealed record LanguageCode
{
    /// <summary>BCP 47 or ISO-style tag.</summary>
    public string Value { get; }

    private LanguageCode(string value) => Value = value;

    /// <summary>
    /// Validates length and returns a trimmed language tag.
    /// </summary>
    public static LanguageCode Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Language code cannot be empty.");

        var trimmed = value.Trim();
        if (trimmed.Length > 10)
            throw new DomainException($"Language code '{trimmed}' is too long (max 10 characters).");

        return new LanguageCode(trimmed);
    }

    /// <summary>Placeholder representing an undetermined language.</summary>
    public static LanguageCode Und => new("und");

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(LanguageCode v) => v.Value;

    /// <inheritdoc />
    public override string ToString() => Value;
}
