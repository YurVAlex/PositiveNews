using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A non-empty, lower-case slug string used for topics.
/// </summary>
public sealed record Slug
{
    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string? value, string fieldName = "Slug")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"'{fieldName}' cannot be empty.");

        return new Slug(value.Trim().ToLowerInvariant());
    }

    public static implicit operator string(Slug v) => v.Value;

    public override string ToString() => Value;
}
