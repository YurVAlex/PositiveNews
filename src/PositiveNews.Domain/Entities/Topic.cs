using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class Topic
{
    private readonly List<ArticleTopic> _articleTopics = [];
    private readonly List<UserTopicFilter> _userTopicFilters = [];

    // For EF Core materialization
    private Topic() { }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation
    public IReadOnlyCollection<ArticleTopic> ArticleTopics => _articleTopics.AsReadOnly();
    public IReadOnlyCollection<UserTopicFilter> UserTopicFilters => _userTopicFilters.AsReadOnly();

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
