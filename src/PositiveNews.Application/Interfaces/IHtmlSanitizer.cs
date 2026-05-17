using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Sanitizes HTML for safe storage and rendering according to allow-lists.
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    /// Removes disallowed tags/attributes and applies structural cleanup to HTML content.
    /// </summary>
    /// <param name="rootNode">Parsed HTML root node.</param>
    /// <param name="rules">Cleaner rules defining allowed tags and stripping patterns.</param>
    /// <returns>Sanitized HTML string.</returns>
    string SanitizeContent(HtmlNode rootNode, CleanerRules rules);

    /// <summary>
    /// Converts HTML to plain text for previews or analysis.
    /// </summary>
    /// <param name="htmlContent">Raw HTML string.</param>
    /// <param name="htmlNode">Optional parsed tree.</param>
    /// <returns>Plain text, or <see langword="null"/> when empty.</returns>
    string? StripToPlainText(string? htmlContent, HtmlNode? htmlNode = null);
}
