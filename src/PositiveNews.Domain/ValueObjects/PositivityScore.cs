using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A sentiment score between 0.0000 and 1.0000 inclusive.
/// </summary>
public sealed record PositivityScore
{
    public decimal Value { get; }

    private PositivityScore(decimal value) => Value = value;

    public static PositivityScore Create(decimal value)
    {
        if (value < 0m || value > 1m)
            throw new InvalidArticleStateException(
                $"PositivityScore must be between 0 and 1 (got {value}).");
        return new PositivityScore(Math.Round(value, 4));
    }

    public static implicit operator decimal(PositivityScore v) => v.Value;

    public override string ToString() => Value.ToString("F4");
}
