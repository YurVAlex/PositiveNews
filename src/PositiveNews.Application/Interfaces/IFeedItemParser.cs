using PositiveNews.Application.DTOs;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemParser
{
    RssFeedItemDto Parse(XElement itemElement);
}
