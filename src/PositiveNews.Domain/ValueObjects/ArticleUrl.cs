using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A validated, absolute article URL.
/// </summary>
public sealed record ArticleUrl
{
    public string Value { get; }

    private ArticleUrl(string value) => Value = value;

    public static ArticleUrl Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidArticleStateException("Article URL cannot be empty.");

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out _))
            throw new InvalidArticleStateException($"Article URL '{value}' is not a valid absolute URL.");

        return new ArticleUrl(value.Trim());
    }

    public static implicit operator string(ArticleUrl v) => v.Value;

    public override string ToString() => Value;
}
