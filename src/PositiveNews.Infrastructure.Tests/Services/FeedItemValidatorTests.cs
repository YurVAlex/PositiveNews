using FluentAssertions;
using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class FeedItemValidatorTests
{
    private static RssFeedItemDto ValidItem() => new()
    {
        Title = "Title",
        Link = "https://example.com/article",
        Description = "Description",
        ContentRaw = "<p>Content</p>"
    };

    private static HtmlNode LongContentNode()
        => HtmlNode.CreateNode("<article>This is long enough article content for validation.</article>");

    private static FeedItemValidationRules EmptyRules()
        => new(new HashSet<string>(), []);

    private static FeedItemValidationRules RulesWithBlockedLinkFragments(params string[] fragments)
        => new(new HashSet<string>(), fragments);

    [Fact]
    public void IsValid_Should_ReturnTrue_When_ItemCompleteAndContentLongEnough()
    {
        var sut = new FeedItemValidator();

        var result = sut.IsValid(ValidItem(), EmptyRules(), LongContentNode());

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(RssFeedItemDto.Title))]
    [InlineData(nameof(RssFeedItemDto.Link))]
    [InlineData(nameof(RssFeedItemDto.Description))]
    [InlineData(nameof(RssFeedItemDto.ContentRaw))]
    public void IsValid_Should_ReturnFalse_When_RequiredStringMissing(string property)
    {
        var sut = new FeedItemValidator();
        var item = property switch
        {
            nameof(RssFeedItemDto.Title) => ValidItem() with { Title = "" },
            nameof(RssFeedItemDto.Link) => ValidItem() with { Link = "   " },
            nameof(RssFeedItemDto.Description) => ValidItem() with { Description = "" },
            nameof(RssFeedItemDto.ContentRaw) => ValidItem() with { ContentRaw = "" },
            _ => ValidItem()
        };

        var result = sut.IsValid(item, EmptyRules(), LongContentNode());

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_AuthorBlocked()
    {
        var sut = new FeedItemValidator();
        var rules = new FeedItemValidationRules(
            new HashSet<string>(["Blocked"], StringComparer.OrdinalIgnoreCase),
            []);

        var result = sut.IsValid(ValidItem() with { Author = "Blocked" }, rules, LongContentNode());

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_ContentNodeInnerTextTooShort()
    {
        var sut = new FeedItemValidator();
        var shortNode = HtmlNode.CreateNode("<article>Too short</article>");

        var result = sut.IsValid(ValidItem(), EmptyRules(), shortNode);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_LinkContainsBlockedFragment()
    {
        var sut = new FeedItemValidator();
        var rules = RulesWithBlockedLinkFragments("photojournal");

        var result = sut.IsValid(
            ValidItem() with { Link = "https://example.com/photojournal/gallery" },
            rules,
            LongContentNode());

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_ContentNodeNull()
    {
        var sut = new FeedItemValidator();

        var result = sut.IsValid(ValidItem(), EmptyRules(), null);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("not-a-url")]
    [InlineData("javascript:alert(1)")]
    public void IsValid_Should_ReturnFalse_When_LinkIsNotAbsoluteHttpOrHttps(string link)
    {
        var sut = new FeedItemValidator();

        var result = sut.IsValid(ValidItem() with { Link = link }, EmptyRules(), LongContentNode());

        result.Should().BeFalse();
    }
}
