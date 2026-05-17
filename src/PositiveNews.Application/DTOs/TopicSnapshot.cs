namespace PositiveNews.Application.DTOs;

/// <summary>
/// Immutable projection of a topic row used when building <see cref="TopicLookup"/>.
/// </summary>
/// <param name="Id">Topic primary key.</param>
/// <param name="Name">Canonical display name.</param>
/// <param name="Slug">Comma/space-separated slug tokens for fuzzy matching.</param>
/// <param name="Description">Optional long description for admin UI.</param>
public sealed record TopicSnapshot(
    int Id,
    string Name,
    string Slug,
    string? Description);
