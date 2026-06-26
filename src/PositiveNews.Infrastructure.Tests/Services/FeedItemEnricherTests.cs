using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging.Abstractions;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class FeedItemEnricherTests
{
    private static FeedItemEnricher CreateSut() => new(NullLogger<FeedItemEnricher>.Instance);

    [Fact]
    public void EnrichTopics_Should_AddDefaultTopics_When_UrlMatchesRule()
    {
        var lookup = new TopicLookup(
            new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase)
            {
                ["Health"] = new TopicSnapshot(1, "Health", "health", null)
            },
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));

        var emptyWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var settings = new IngestionSettingsSnapshot(
            PositivityAnalyzerTestLexicon.Create(
                positiveWords: emptyWords,
                negativeWords: emptyWords,
                positivePhrases: emptyWords,
                negativePhrases: emptyWords,
                negationWords: emptyWords,
                intensifierWords: emptyWords,
                negationLookbackTokens: 1,
                intensifierLookbackTokens: 1,
                intensifierMultiplier: 1m,
                phrasePolarityWeight: 1m),
            new CleanerRules([], [], [], [], new HashSet<string>(), new HashSet<string>()),
            new FeedItemValidationRules(new HashSet<string>(), []),
            new Dictionary<string, SourceIngestionRules>(StringComparer.OrdinalIgnoreCase)
            {
                ["s1"] = new SourceIngestionRules("news.example", ["Health"])
            });

        var dto = new RssFeedItemDto { Topics = [] };

        var result = CreateSut().EnrichTopics("https://news.example.com/feed", dto, lookup, settings);

        result.Topics.Should().Contain("Health");
    }

    [Fact]
    public void AddHeroImage_Should_PrependImageTag_When_NoHeroPresent()
    {
        var dto = new RssFeedItemDto { ContentRaw = "<p>body</p>" };
        var tag = "<img class=\"img-fluid w-100\" src=\"x\" />";

        var result = CreateSut().AddHeroImage(dto, tag, HtmlNode.CreateNode("<div><p>body</p></div>"));

        result.ContentRaw.Should().StartWith(tag);
    }

    [Fact]
    public void AddHeroImage_Should_ReturnOriginal_When_ContentEmpty()
    {
        var dto = new RssFeedItemDto { ContentRaw = "" };

        var result = CreateSut().AddHeroImage(dto, "<img />", null);

        result.Should().BeSameAs(dto);
    }

    [Fact]
    public void AddHeroImage_Should_PreserveDto_When_AlreadyHasHeroClass()
    {
        var html = "<div><img class=\"img-fluid w-100 rounded\" src=\"x\"/></div><p>more</p>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var dto = new RssFeedItemDto { ContentRaw = html };

        var result = CreateSut().AddHeroImage(dto, "<img />", doc.DocumentNode);

        result.ContentRaw.Should().Be(html);
    }
}
