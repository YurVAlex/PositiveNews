using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Configuration;

public class TopicConfiguration
{
    public List<TopicEntry> Topics { get; set; } = new();
}

public class TopicEntry
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Topic ToEntity()
    {
        return new Topic
        {
            Name = Name,
            Slug = Slug,
            Description = Description
        };
    }
}