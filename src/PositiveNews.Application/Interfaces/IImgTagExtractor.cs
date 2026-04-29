using HtmlAgilityPack;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IImgTagExtractor
{
    string? ExtractImgTag(XElement itemElement, string feedUrl, HtmlNode? contentNode,
        HtmlNode? descriptionNode, string? defaultThumbnailHtml);
}
