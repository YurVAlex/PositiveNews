using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PositiveNews.Application.Ingestion;
using PositiveNews.Infrastructure.Services;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Tests.Services;

public class FeedItemParserTests
{
    private static FeedItemParser CreateSut() => new(NullLogger<FeedItemParser>.Instance);

    [Fact]
    public void Parse_Should_MapFields_When_ItemWellFormed()
    {
        var item = XElement.Parse(
            """
            <item xmlns:content="http://purl.org/rss/1.0/modules/content/" xmlns:dc="http://purl.org/dc/elements/1.1/">
              <title> Headline </title>
              <link>https://news.example.com/p/1</link>
              <description>Summary text.</description>
              <content:encoded><![CDATA[<p>Body</p>]]></content:encoded>
              <dc:creator>Jane</dc:creator>
              <pubDate>Mon, 01 Jan 2026 12:00:00 GMT</pubDate>
              <category>World</category>
              <guid isPermaLink="false">unique-guid</guid>
            </item>
            """);

        var dto = CreateSut().Parse(item);

        dto.Title.Should().Be("Headline");
        dto.Link.Should().Be("https://news.example.com/p/1");
        dto.Description.Should().Be("Summary text.");
        dto.ContentRaw.Should().Be("<p>Body</p>");
        dto.Author.Should().Be("Jane");
        dto.ExternalId.Should().Be("unique-guid");
        dto.Topics.Should().Contain("World");
    }

    [Fact]
    public void Parse_Should_UseDefaultTopic_When_NoCategories()
    {
        var item = XElement.Parse(
            """
            <item>
              <title>T</title>
              <link>https://a.com</link>
              <description>D</description>
            </item>
            """);

        var dto = CreateSut().Parse(item);

        dto.Topics.Should().Equal(IngestionCatalogConstants.DefaultTopicName);
    }

    [Fact]
    public void Parse_Should_AllowEmptyOptionalFields_When_MinimalItem()
    {
        var item = XElement.Parse(
            """
            <item>
              <title>T</title>
              <link>https://a.com</link>
              <description>D</description>
            </item>
            """);

        var dto = CreateSut().Parse(item);

        dto.ContentRaw.Should().BeEmpty();
        dto.Author.Should().BeNull();
        dto.ExternalId.Should().BeNull();
    }
}
