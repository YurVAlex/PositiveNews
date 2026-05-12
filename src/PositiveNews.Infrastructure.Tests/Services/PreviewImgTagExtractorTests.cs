using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging.Abstractions;
using PositiveNews.Infrastructure.Services;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Tests.Services;

public class PreviewImgTagExtractorTests
{
    private static PreviewImgTagExtractor CreateSut() => new(NullLogger<PreviewImgTagExtractor>.Instance);

    [Fact]
    public void ExtractImgTag_Should_ReturnMediaThumbnail_When_MrssThumbnailPresent()
    {
        XNamespace media = "http://search.yahoo.com/mrss/";
        var item = new XElement("item",
            new XElement(media + "thumbnail",
                new XAttribute("url", "https://cdn.example.com/t.jpg"),
                new XAttribute("width", "120"),
                new XAttribute("height", "80")));

        var tag = CreateSut().ExtractImgTag(item, "https://feed", null, null, null);

        tag.Should().Contain("https://cdn.example.com/t.jpg");
        tag.Should().Contain("img");
    }

    [Fact]
    public void ExtractImgTag_Should_ReturnNull_When_NoImageAndNoDefault()
    {
        var item = new XElement("item");

        var tag = CreateSut().ExtractImgTag(item, "https://feed", null, null, null);

        tag.Should().BeNull();
    }

    [Fact]
    public void ExtractImgTag_Should_UseDefaultThumbnail_When_NoOtherImage()
    {
        var item = new XElement("item");
        var def = "<img src=\"https://default\" />";

        var tag = CreateSut().ExtractImgTag(item, "https://feed", null, null, def);

        tag.Should().Be(def);
    }

    [Fact]
    public void ExtractImgTag_Should_ParseImgFromHtml_When_ContentHasImage()
    {
        var item = new XElement("item");
        var html = new HtmlDocument();
        html.LoadHtml("<div><img width=\"800\" height=\"600\" src=\"https://first.com/a.jpg\"/></div>");

        var tag = CreateSut().ExtractImgTag(item, "https://feed", html.DocumentNode, null, null);

        tag.Should().Contain("first.com");
    }
}
