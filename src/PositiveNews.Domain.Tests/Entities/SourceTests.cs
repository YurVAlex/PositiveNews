using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class SourceTests
{
    [Fact]
    public void Create_Should_SetFieldsAndTrimStrings_When_ValidInputProvided()
    {
        var before = DateTime.UtcNow;
        var source = Source.Create(
            name: "  CNN  ",
            baseUrl: " https://cnn.com ",
            feedUrl: " https://cnn.com/rss ",
            description: " desc ",
            logoUrl: " logo.png ",
            trustScore: 0.9m,
            defaultLanguageCode: " en ",
            defaultThumbnailHtml: " <img/> ");

        source.Name.Should().Be("CNN");
        source.BaseUrl.Should().Be("https://cnn.com");
        source.FeedUrl.Should().Be("https://cnn.com/rss");
        source.Description.Should().Be(" desc ");
        source.LogoUrl.Should().Be("logo.png");
        source.TrustScore.Should().Be(0.9m);
        source.DefaultLanguageCode.Should().Be("en");
        source.DefaultThumbnailHtml.Should().Be(" <img/> ");
        source.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        source.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidSourceStateException_When_NameInvalid(string? name)
    {
        var act = () => Source.Create(name!, "http://x");

        act.Should().Throw<InvalidSourceStateException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidSourceStateException_When_BaseUrlInvalid(string? url)
    {
        var act = () => Source.Create("Name", url!);

        act.Should().Throw<InvalidSourceStateException>();
    }

    [Fact]
    public void Create_Should_ThrowInvalidSourceStateException_When_TrustScoreNegative()
    {
        var act = () => Source.Create("Name", "http://x", trustScore: -0.1m);

        act.Should().Throw<InvalidSourceStateException>();
    }

    [Fact]
    public void Deactivate_Should_MarkInactiveAndPreventSecondCall_When_AlreadyInactive()
    {
        var s = Source.Create("Name", "http://x");

        s.Deactivate(1);

        s.IsActive.Should().BeFalse();
        s.ModeratedBy.Should().Be(1);

        var act = () => s.Deactivate(1);

        act.Should().Throw<InvalidSourceStateException>();
    }

    [Fact]
    public void UpdateFeedUrl_Should_SetTrimmedUrl_When_ValidInput()
    {
        var s = Source.Create("Name", "http://x", "old");

        s.UpdateFeedUrl(" http://new ");

        s.FeedUrl.Should().Be("http://new");
    }

    [Fact]
    public void UpdateFeedUrl_Should_ThrowInvalidSourceStateException_When_Empty()
    {
        var s = Source.Create("Name", "http://x");

        var act = () => s.UpdateFeedUrl("");

        act.Should().Throw<InvalidSourceStateException>();
    }

    [Fact]
    public void UpdateDetails_Should_UpdateDescriptionAndLogo_When_Called()
    {
        var s = Source.Create("Name", "http://x");

        s.UpdateDetails("new desc", "  logo.png  ");

        s.Description.Should().Be("new desc");
        s.LogoUrl.Should().Be("logo.png");
    }
}
