using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A validated, absolute article URL.
/// </summary>
public sealed record ArticleUrl
{
    /// <summary>Trimmed absolute URI string.</summary>
    public string Value { get; }

    private ArticleUrl(string value) => Value = value;

    /// <summary>
    /// Validates non-empty input and ensures it parses as an absolute URI.
    /// </summary>
    public static ArticleUrl Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidArticleStateException("Article URL cannot be empty.");

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out _))
            throw new InvalidArticleStateException($"Article URL '{value}' is not a valid absolute URL.");

        return new ArticleUrl(value.Trim());
    }

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(ArticleUrl v) => v.Value;

    /// <inheritdoc />
    public override string ToString() => Value;
}
