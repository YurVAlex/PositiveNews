using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

public class FeedItemEnricher : IFeedItemEnricher
{
    private readonly ILogger<FeedItemEnricher> _logger;

    public FeedItemEnricher(ILogger<FeedItemEnricher> logger)
    {
        _logger = logger;
    }

    public void EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup)
    {
        var result = new HashSet<string>(dto.Topics, StringComparer.OrdinalIgnoreCase);

        void Add(string name)
        {
            if (lookup.ByName.ContainsKey(name))
                result.Add(name);
        }

        if (feedUrl.Contains("nvidia", StringComparison.OrdinalIgnoreCase))
            Add("Technology");

        if (feedUrl.Contains("nasa", StringComparison.OrdinalIgnoreCase))
        {
            Add("Space");
            Add("Technology");
            Add("Science");
        }

        if (feedUrl.Contains("thisiscolossal", StringComparison.OrdinalIgnoreCase) ||
            feedUrl.Contains("designyoutrust", StringComparison.OrdinalIgnoreCase))
        {
            Add("Arts & Culture");
        }

        if (feedUrl.Contains("tinybuddha", StringComparison.OrdinalIgnoreCase))
            Add("Psychology");

        var expandedTopics = new HashSet<string>(result, StringComparer.OrdinalIgnoreCase);
        foreach (var topicName in result.ToList())
        {
            if (!lookup.ByName.TryGetValue(topicName, out var topic))
                continue;

            var slugWords = topic.Slug
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim());

            foreach (var word in slugWords)
            {
                if (lookup.ByName.TryGetValue(word, out var related))
                    result.Add(related.Name);
            }
        }

        foreach (var expanded in expandedTopics)
            result.Add(expanded);

        if (result.Count == 0)
            Add("Default");

        dto.Topics = result.ToList();
    }

    public string AddHeroImage(string contentRaw, string? imageTag, HtmlNode? contentNode)
    {
        if (string.IsNullOrWhiteSpace(contentRaw) ||
            string.IsNullOrWhiteSpace(imageTag) ||
            ContainsHeroImage(contentNode))
        {
            return contentRaw;
        }

        return string.Concat(imageTag, contentRaw);
    }

    private bool ContainsHeroImage(HtmlNode? htmlNode)
    {
        try
        {
            if (htmlNode == null)
                return false;

            var images = htmlNode.SelectNodes(".//img");
            if (images == null || images.Count == 0)
                return false;

            return images.Any(img =>
            {
                var classAttr = img.GetAttributeValue("class", string.Empty);
                return classAttr.Contains("img-fluid", StringComparison.OrdinalIgnoreCase) &&
                       classAttr.Contains("w-100", StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking for hero image in HTML content");
            return false;
        }
    }
}
