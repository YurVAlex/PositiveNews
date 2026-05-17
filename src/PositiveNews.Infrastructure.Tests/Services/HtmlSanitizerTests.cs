using FluentAssertions;
using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class HtmlSanitizerTests
{
    private static HtmlSanitizer CreateSut() => new(new MediaEmbedder());

    private static CleanerRules RulesWithAllowedTags(params string[] tags) => new(
        StopProcessingPatterns: [],
        RemoveNodePatterns: [],
        RemoveDivClassPatterns: [],
        ShouldRemoveParagraphPatterns: [],
        AllowedTags: new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase),
        AttributesToRemove: new HashSet<string>(["onclick"], StringComparer.OrdinalIgnoreCase));

    [Fact]
    public void SanitizeContent_Should_RemoveScriptTags_When_PresentInTree()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<p onclick=\"evil()\">Hi<script>x</script></p>");
        var sut = CreateSut();

        var html = sut.SanitizeContent(doc.DocumentNode, RulesWithAllowedTags("p"));

        html.Should().NotContain("script");
        html.Should().NotContain("onclick");
        html.Should().Contain("Hi");
    }

    [Fact]
    public void SanitizeContent_Should_PreserveAllowedParagraph_When_InAllowedList()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<p class=\"x\">Hello world</p>");
        var sut = CreateSut();

        var html = sut.SanitizeContent(doc.DocumentNode, RulesWithAllowedTags("p"));

        html.Should().Contain("Hello world");
        html.Should().Contain("<p");
    }

    [Fact]
    public void StripToPlainText_Should_ReturnEmpty_When_HtmlEmpty()
    {
        var sut = CreateSut();

        sut.StripToPlainText("   ", null).Should().Be("   ");
    }

    [Fact]
    public void StripToPlainText_Should_UseInnerText_When_NodeProvided()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<div><span>T</span></div>");
        var sut = CreateSut();

        sut.StripToPlainText(null, doc.DocumentNode).Should().Contain("T");
    }
}
