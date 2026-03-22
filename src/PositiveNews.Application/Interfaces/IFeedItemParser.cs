using PositiveNews.Application.DTOs;
using System.Xml.Linq;

public interface IFeedItemParser
{
    RssFeedItemDto Parse(XElement itemElement);
}