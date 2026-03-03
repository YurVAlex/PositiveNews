namespace PositiveNews.Domain.Entities;

public class Topic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<ArticleTopic> ArticleTopics { get; set; } = new List<ArticleTopic>();
    public ICollection<UserTopicFilter> UserTopicFilters { get; set; } = new List<UserTopicFilter>();
}