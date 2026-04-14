using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemValidator
{
    bool IsValid(RssFeedItemDto item, HtmlNode? contentNode);
}