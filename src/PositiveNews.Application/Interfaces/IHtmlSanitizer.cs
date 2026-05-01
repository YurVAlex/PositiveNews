using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IHtmlSanitizer
{
    string SanitizeContent(HtmlNode rootNode, CommonIngestionRules rules);
    string? StripToPlainText(string? htmlContent, HtmlNode? htmlNode = null);
}
