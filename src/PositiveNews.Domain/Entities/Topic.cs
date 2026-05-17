using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// A navigational topic used for tagging articles and user filters (name + URL slug).
/// </summary>
public class Topic
{
    private readonly List<ArticleTopic> _articleTopics = [];
    private readonly List<UserTopicFilter> _userTopicFilters = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private Topic() { }

    /// <summary>Primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Human-readable topic label.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Lower-case slug for URLs and lookups.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Optional longer description.</summary>
    public string? Description { get; private set; }

    /// <summary>Article-topic associations.</summary>
    public IReadOnlyCollection<ArticleTopic> ArticleTopics => _articleTopics.AsReadOnly();

    /// <summary>Users who filter their feed to this topic.</summary>
    public IReadOnlyCollection<UserTopicFilter> UserTopicFilters => _userTopicFilters.AsReadOnly();

    /// <summary>
    /// Creates a topic with trimmed name and normalized slug.
    /// </summary>
    public static Topic Create(string name, string slug, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Topic name cannot be empty.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Topic slug cannot be empty.");

        return new Topic
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description
        };
    }

    /// <summary>
    /// Updates display name and/or description; name must remain non-empty when provided.
    /// </summary>
    public void Update(string? name, string? description)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Topic name cannot be empty.");
            Name = name.Trim();
        }
        Description = description;
    }
}
