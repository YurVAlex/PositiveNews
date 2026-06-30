using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Configuration;

/// <summary>
/// Strongly typed binding for the <c>SeedData</c> configuration section used at startup.
/// </summary>
public class SeedDataConfiguration
{
    /// <summary>Initial roles to insert when the database is empty.</summary>
    public List<RoleEntry> Roles { get; set; } = [];

    /// <summary>Initial news sources to insert when the database is empty.</summary>
    public List<SourceEntry> Sources { get; set; } = [];

    /// <summary>Initial topics to insert when the database is empty.</summary>
    public List<TopicEntry> Topics { get; set; } = [];
}

/// <summary>
/// Role seed row mapped to a domain <see cref="Role"/>.
/// </summary>
public class RoleEntry
{
    /// <summary>Unique role name (e.g. Admin).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="Role"/> entity from this entry.
    /// </summary>
    /// <returns>A constructed role instance.</returns>
    public Role ToEntity() => Role.Create(Name);
}

/// <summary>
/// News source seed row mapped to a domain <see cref="Source"/>.
/// </summary>
public class SourceEntry
{
    /// <summary>Display name of the outlet.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Canonical site URL.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>RSS or Atom feed URL, if any.</summary>
    public string? FeedUrl { get; set; }

    /// <summary>Optional marketing or catalog description.</summary>
    public string? Description { get; set; }

    /// <summary>Optional logo URL.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Editorial trust weight used by the application.</summary>
    public decimal TrustScore { get; set; } = 1.0m;

    /// <summary>Default BCP 47 language tag for articles from this source.</summary>
    public string DefaultLanguageCode { get; set; } = LanguageDefaults.SourceDefault;

    /// <summary>Optional default hero thumbnail HTML when feeds omit images.</summary>
    public string? DefaultThumbnailHtml { get; set; }

    /// <summary>
    /// Creates a new <see cref="Source"/> entity from this entry.
    /// </summary>
    /// <returns>A constructed source instance.</returns>
    public Source ToEntity() => Source.Create(
        Name, BaseUrl, FeedUrl, Description, LogoUrl,
        TrustScore, DefaultLanguageCode, DefaultThumbnailHtml);
}

/// <summary>
/// Topic seed row mapped to a domain <see cref="Topic"/>.
/// </summary>
public class TopicEntry
{
    /// <summary>Human-readable topic label.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL- and lookup-friendly slug (may contain comma-separated slug words).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Optional description shown in admin or UI.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Creates a new <see cref="Topic"/> entity from this entry.
    /// </summary>
    /// <returns>A constructed topic instance.</returns>
    public Topic ToEntity() => Topic.Create(Name, Slug, Description);
}
