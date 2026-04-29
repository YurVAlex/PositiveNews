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

    public RssFeedItemDto EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup,
        IngestionSettingsSnapshot settings)
    {
        var result = new HashSet<string>(dto.Topics, StringComparer.OrdinalIgnoreCase);

        void Add(string name)
        {
            if (lookup.ByName.ContainsKey(name))
                result.Add(name);
        }

        foreach (var rule in settings.Sources.Values)
        {
            if (feedUrl.Contains(rule.UrlContains, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var topicName in rule.DefaultTopics)
                    Add(topicName);
            }
        }

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
                    expandedTopics.Add(related.Name);
            }
        }

        foreach (var expanded in expandedTopics)
            result.Add(expanded);

        if (result.Count == 0)
            Add("Default");

        return dto with { Topics = result.ToList() };
    }

    public RssFeedItemDto AddHeroImage(RssFeedItemDto dto, string? imageTag, HtmlNode? contentNode)
    {
        if (string.IsNullOrWhiteSpace(dto.ContentRaw) ||
            string.IsNullOrWhiteSpace(imageTag) ||
            ContainsHeroImage(contentNode))
        {
            return dto;
        }

        return dto with { ContentRaw = string.Concat(imageTag, dto.ContentRaw) };
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
