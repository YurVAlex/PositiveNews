using FluentAssertions;
using HtmlAgilityPack;
using PositiveNews.Infrastructure.Services;
using System.Text;

namespace PositiveNews.Infrastructure.Tests.Services;

public class MediaEmbedderTests
{
    private readonly MediaEmbedder _sut = new();

    [Fact]
    public void TryEmbed_Should_ReturnTrueAndEmbedYouTube_When_AnchorHrefIsYoutube()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("""<a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ">link</a>""");
        var a = doc.DocumentNode.SelectSingleNode("//a")!;
        var sb = new StringBuilder();

        var ok = _sut.TryEmbed(a, sb);

        ok.Should().BeTrue();
        sb.ToString().Should().Contain("youtube.com/embed/dQw4w9WgXcQ");
    }

    [Fact]
    public void TryEmbed_Should_ReturnFalse_When_AnchorNotYoutube()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("""<a href="https://example.com/">x</a>""");
        var a = doc.DocumentNode.SelectSingleNode("//a")!;
        var sb = new StringBuilder();

        var ok = _sut.TryEmbed(a, sb);

        ok.Should().BeFalse();
        sb.ToString().Should().BeEmpty();
    }

    [Fact]
    public void EmbedImage_Should_AppendFluidClasses_When_LargeImage()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("""<img src="https://x.com/i.jpg" />""");
        var img = doc.DocumentNode.SelectSingleNode("//img")!;
        var sb = new StringBuilder();

        _sut.EmbedImage(img, sb);

        sb.ToString().Should().Contain("img-fluid").And.Contain("w-100");
    }

    [Fact]
    public void CreateYouTubeEmbed_Should_ContainVideoId_When_ValidId()
    {
        var html = _sut.CreateYouTubeEmbed("dQw4w9WgXcQ");

        html.Should().Contain("embed/dQw4w9WgXcQ");
    }
}
