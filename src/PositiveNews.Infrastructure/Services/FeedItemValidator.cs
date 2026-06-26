using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Validates that a parsed RSS item has required fields, acceptable author, and sufficiently rich HTML body text.
/// </summary>
public class FeedItemValidator : IFeedItemValidator
{
    /// <inheritdoc />
    public bool IsValid(RssFeedItemDto item, FeedItemValidationRules rules, HtmlNode? contentNode)
    {
        if (string.IsNullOrWhiteSpace(item.Title) ||
            string.IsNullOrWhiteSpace(item.Link) ||
            string.IsNullOrWhiteSpace(item.Description) ||
            string.IsNullOrWhiteSpace(item.ContentRaw))
            return false;

        if (!string.IsNullOrWhiteSpace(item.Author) &&
            rules.InvalidAuthors.Contains(item.Author))
            return false;

        if (contentNode == null ||
            contentNode.InnerHtml == null ||
            string.IsNullOrWhiteSpace(contentNode.InnerText) ||
            contentNode.InnerText.Length < 25)
        {
            return false;
        }

        foreach (var fragment in rules.InvalidLinkContains)
        {
            if (item.Link.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
