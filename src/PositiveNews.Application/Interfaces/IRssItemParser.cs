using PositiveNews.Application.DTOs;
using System.Xml.Linq;

public interface IRssItemParser
{
    RssFeedItemDto Parse(XElement itemElement);
}