using FluentAssertions;
using HtmlAgilityPack;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class TextNormalizerTests
{
    private readonly TextNormalizer _sut = new();

    [Fact]
    public void NormalizeTitle_Should_Truncate_When_Exceeds500Characters()
    {
        var longTitle = new string('a', 520);

        var result = _sut.NormalizeTitle(longTitle);

        result.Should().HaveLength(500);
    }

    [Fact]
    public void NormalizeDescription_Should_ReturnEmpty_When_InputWhitespace()
    {
        _sut.NormalizeDescription("   ").Should().BeEmpty();
    }

    [Fact]
    public void NormalizeContent_Should_TrimOutput_When_TrailingWhitespace()
    {
        var result = _sut.NormalizeContent("<p>x</p>  ");

        result.Should().NotEndWith(" ");
    }

    [Fact]
    public void NormalizeDescription_Should_ReturnPlainFromParagraphs_When_HtmlDescription()
    {
        var html = "<p>First sentence.</p><p>Second sentence.</p>";

        var result = _sut.NormalizeDescription(html);

        result.Should().Contain("First sentence");
    }
}
