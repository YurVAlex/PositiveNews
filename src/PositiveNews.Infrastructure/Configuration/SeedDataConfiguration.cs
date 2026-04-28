using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Configuration;

public class SeedDataConfiguration
{
    public List<RoleEntry> Roles { get; set; } = [];
    public List<SourceEntry> Sources { get; set; } = [];
    public List<TopicEntry> Topics { get; set; } = [];
}

public class RoleEntry
{
    public string Name { get; set; } = string.Empty;

    public Role ToEntity() => Role.Create(Name);
}

public class SourceEntry
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? FeedUrl { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public decimal TrustScore { get; set; } = 1.0m;
    public string DefaultLanguageCode { get; set; } = "en";
    public string? DefaultThumbnailHtml { get; set; }

    public Source ToEntity() => Source.Create(
        Name, BaseUrl, FeedUrl, Description, LogoUrl,
        TrustScore, DefaultLanguageCode, DefaultThumbnailHtml);
}

public class TopicEntry
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Topic ToEntity() => Topic.Create(Name, Slug, Description);
}
