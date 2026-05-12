using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A sentiment score between 0.0000 and 1.0000 inclusive.
/// </summary>
public sealed record PositivityScore
{
    /// <summary>Rounded to four decimal places.</summary>
    public decimal Value { get; }

    private PositivityScore(decimal value) => Value = value;

    /// <summary>
    /// Validates range, rounds to four decimals, and wraps the value.
    /// </summary>
    public static PositivityScore Create(decimal value)
    {
        if (value < 0m || value > 1m)
            throw new InvalidArticleStateException(
                $"PositivityScore must be between 0 and 1 (got {value}).");
        return new PositivityScore(Math.Round(value, 4));
    }

    /// <summary>Implicit conversion to decimal.</summary>
    public static implicit operator decimal(PositivityScore v) => v.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString("F4");
}
