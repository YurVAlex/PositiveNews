using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Net;

namespace PositiveNews.Infrastructure.Services;

public class FeedItemValidator : IFeedItemValidator
{
    public bool IsValid(RssFeedItemDto item, HtmlNode? contentNode) //TODO: Additional validation
    {
        if (string.IsNullOrWhiteSpace(item.Title) ||
            string.IsNullOrWhiteSpace(item.Link) ||
            string.IsNullOrWhiteSpace(item.Description) ||
            string.IsNullOrWhiteSpace(item.ContentRaw))
            return false;

        if (item.Author == "tinybuddha")
            return false;

        if (contentNode == null ||
            contentNode.InnerHtml == null ||
            string.IsNullOrWhiteSpace(contentNode.InnerText) ||
            contentNode.InnerText.Length < 25)
        {
            return false;
        }

        return true;
    }
}