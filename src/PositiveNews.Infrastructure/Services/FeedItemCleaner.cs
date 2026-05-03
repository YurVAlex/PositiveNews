using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

public class FeedItemCleaner : IFeedItemCleaner
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ITopicNormalizer _topicNormalizer;

    public FeedItemCleaner(
        IHtmlSanitizer htmlSanitizer,
        ITextNormalizer textNormalizer,
        ITopicNormalizer topicNormalizer)
    {
        _htmlSanitizer = htmlSanitizer;
        _textNormalizer = textNormalizer;
        _topicNormalizer = topicNormalizer;
    }

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

    public IReadOnlyList<string> CleanTopics(IReadOnlyList<string> topics, TopicLookup lookup)
    {
        return _topicNormalizer.NormalizeTopics(topics, lookup);
    }

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
