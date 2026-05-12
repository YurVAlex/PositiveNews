using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.ValueObjects;

/// <summary>
/// A non-empty, lower-case slug string used for topics.
/// </summary>
public sealed record Slug
{
    /// <summary>Normalized slug text.</summary>
    public string Value { get; }

    private Slug(string value) => Value = value;

    /// <summary>
    /// Validates and returns a slug with trimmed, lower-invariant content.
    /// </summary>
    /// <param name="value">Raw slug input.</param>
    /// <param name="fieldName">Name used in error messages.</param>
    public static Slug Create(string? value, string fieldName = "Slug")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"'{fieldName}' cannot be empty.");

        return new Slug(value.Trim().ToLowerInvariant());
    }

    /// <summary>Implicit conversion to string for convenience.</summary>
    public static implicit operator string(Slug v) => v.Value;

    /// <inheritdoc />
    public override string ToString() => Value;
}
