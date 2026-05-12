using FluentAssertions;
using HtmlAgilityPack;
using NSubstitute;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class FeedItemCleanerTests
{
    private static TopicLookup EmptyLookup()
        => new(
            new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));

    private static CleanerRules MinimalRules() => new(
        StopProcessingPatterns: [],
        RemoveNodePatterns: [],
        RemoveDivClassPatterns: [],
        ShouldRemoveParagraphPatterns: [],
        AllowedTags: new HashSet<string>(["p"], StringComparer.OrdinalIgnoreCase),
        AttributesToRemove: new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    [Fact]
    public void Clean_Should_DelegateToSanitizerAndNormalizers_When_DtoAndRulesProvided()
    {
        var html = Substitute.For<IHtmlSanitizer>();
        html.SanitizeContent(Arg.Any<HtmlNode>(), Arg.Any<CleanerRules>()).Returns("<p>sanitized</p>");
        var text = new TextNormalizer();
        var topic = new TopicNormalizer();
        var sut = new FeedItemCleaner(html, text, topic);
        var dto = new RssFeedItemDto
        {
            Title = "  Hello  ",
            Description = "  Desc  ",
            ContentRaw = "<div>x</div>",
            Topics = ["news"]
        };

        var cleaned = sut.Clean(dto, EmptyLookup(), MinimalRules(), null);

        cleaned.Title.Should().Be("  Hello  ");
        cleaned.Description.Should().NotBeNull();
        cleaned.ContentRaw.Should().Be("<p>sanitized</p>");
        html.Received(1).SanitizeContent(Arg.Any<HtmlNode>(), Arg.Any<CleanerRules>());
    }

    [Fact]
    public void Clean_Should_HandleEmptyRawContent_When_NoContentNode()
    {
        var html = Substitute.For<IHtmlSanitizer>();
        html.SanitizeContent(Arg.Any<HtmlNode>(), Arg.Any<CleanerRules>()).Returns("");
        var sut = new FeedItemCleaner(html, new TextNormalizer(), new TopicNormalizer());
        var dto = new RssFeedItemDto
        {
            Title = "T",
            Description = "D",
            ContentRaw = "",
            Topics = []
        };

        var cleaned = sut.Clean(dto, EmptyLookup(), MinimalRules(), null);

        cleaned.ContentRaw.Should().BeEmpty();
    }

    [Fact]
    public void StripInnerHtmlWords_Should_UseSanitizer_When_NodeProvided()
    {
        var html = Substitute.For<IHtmlSanitizer>();
        html.StripToPlainText(Arg.Any<string?>(), Arg.Any<HtmlNode?>()).Returns("plain");
        var sut = new FeedItemCleaner(html, new TextNormalizer(), new TopicNormalizer());
        var node = HtmlNode.CreateNode("<p>inner</p>");

        var text = sut.StripInnerHtmlWords("<p>x</p>", node);

        text.Should().Be("plain");
    }
}
