using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Normalizes titles, descriptions, topics, and HTML body content for ingested RSS items.
/// </summary>
public class FeedItemCleaner : IFeedItemCleaner
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ITopicNormalizer _topicNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedItemCleaner"/> class.
    /// </summary>
    /// <param name="htmlSanitizer">Sanitizes HTML content trees.</param>
    /// <param name="textNormalizer">Plain-text and lightweight HTML cleanup.</param>
    /// <param name="topicNormalizer">Maps raw category strings to catalog topics.</param>
    public FeedItemCleaner(
        IHtmlSanitizer htmlSanitizer,
        ITextNormalizer textNormalizer,
        ITopicNormalizer topicNormalizer)
    {
        _htmlSanitizer = htmlSanitizer;
        _textNormalizer = textNormalizer;
        _topicNormalizer = topicNormalizer;
    }

    /// <inheritdoc />
    public RssFeedItemDto Clean(RssFeedItemDto dto, TopicLookup lookup, CleanerRules rules, HtmlNode? rawContentNode)
    {
        return dto with
        {
            Description = _textNormalizer.NormalizeDescription(dto.Description),
            ContentRaw = CleanContent(dto.ContentRaw, rawContentNode, rules),
            Title = _textNormalizer.NormalizeTitle(dto.Title),
            Topics = _topicNormalizer.NormalizeTopics(dto.Topics, lookup)
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<string> CleanTopics(IReadOnlyList<string> topics, TopicLookup lookup)
    {
        return _topicNormalizer.NormalizeTopics(topics, lookup);
    }

    /// <inheritdoc />
    public string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null)
    {
        return _htmlSanitizer.StripToPlainText(htmlContent, htmlNode);
    }

    private string CleanContent(string rawContent, HtmlNode? rawContentNode, CleanerRules rules)
    {
        var rootNode = rawContentNode ?? LoadDocument(rawContent).DocumentNode;
        var sanitized = _htmlSanitizer.SanitizeContent(rootNode, rules);
        return _textNormalizer.NormalizeContent(sanitized);
    }

    private static HtmlDocument LoadDocument(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
}
