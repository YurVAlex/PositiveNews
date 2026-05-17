using HtmlAgilityPack;
using PositiveNews.Application.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Normalizes titles, summaries, and article HTML by trimming boilerplate and enforcing length limits.
/// </summary>
public class TextNormalizer : ITextNormalizer
{
    private static readonly Regex StrongTildeRegex = new(
        @"<strong>\s*(?<left>[^~<]+?)\s*~\s*[^<]*\s*</strong>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AppearedFirstOnRegex = new(
        @"<a[^>]*>.*?<\/a>\s*appeared first on\s*<a[^>]*>.*?<\/a>\.?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OptimistEditorialRegex = new(
        @"BY THE OPTIMIST DAILY(?:'S|'S)? EDITORIAL TEAM",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ThePostRegex = new(
        @"The post\s*<a[^>]*>.*?<\/a>\s*first appeared on\s*<a[^>]*>.*?<\/a>\.?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex RemoveTrailingPostLinksRegex = new(
        @"<\/p>\s*The post\s*<a.*?<\/a>.*",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex AppearedFirstOnTextRegex = new(
        @"appeared first on",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OptimistDailyTextRegex = new(
        @"BY THE OPTIMIST DAILY",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StartsWithThePostRegex = new(
        @"^The post ",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ThePostSentenceRegex = new(
        @"The post .*? first appeared on .*?\.",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex OptimistDailySentenceRegex = new(
        @"BY THE OPTIMIST DAILY.*?(?=\.)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public string NormalizeContent(string htmlContent)
    {
        var cleaned = RemoveTrailingPostLinksRegex.Replace(htmlContent, "");
        return RemoveTildeAuthor(cleaned).Trim();
    }

    /// <inheritdoc />
    public string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var html = WebUtility.HtmlDecode(description);

        html = ThePostRegex.Replace(html, "");
        html = StrongTildeRegex.Replace(html, "<strong>${left}</strong>");
        html = OptimistEditorialRegex.Replace(html, "");
        html = AppearedFirstOnRegex.Replace(html, "");

        if (!ContainsHtmlMarkup(html))
            return TrimAfterLastDot(html);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var paragraphs = doc.DocumentNode.SelectNodes(".//p");

        if (paragraphs == null || paragraphs.Count == 0)
            return TrimAfterLastDot(doc.DocumentNode.InnerText);

        var texts = paragraphs
            .Select(p => HtmlEntity.DeEntitize(p.InnerText).Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t =>
                !AppearedFirstOnTextRegex.IsMatch(t) &&
                !OptimistDailyTextRegex.IsMatch(t) &&
                !StartsWithThePostRegex.IsMatch(t));

        var result = string.Join(" ", texts);

        result = ThePostSentenceRegex.Replace(result, "");
        result = OptimistDailySentenceRegex.Replace(result, "");
        result = result.Length > 1999 ? result[..1999] : result;

        return TrimAfterLastDot(result);
    }

    /// <inheritdoc />
    public string NormalizeTitle(string title)
    {
        return title.Length > 500 ? title[..500] : title;
    }

    private static string RemoveTildeAuthor(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        return StrongTildeRegex.Replace(html, "<strong>${left}</strong>");
    }

    private static string TrimAfterLastDot(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var lastDotIndex = text.LastIndexOf('.');

        if (lastDotIndex > 0)
            return text.Substring(0, lastDotIndex + 1).Trim();

        return text.Trim();
    }

    private static bool ContainsHtmlMarkup(string input)
    {
        return HtmlTagRegex.IsMatch(input);
    }
}
