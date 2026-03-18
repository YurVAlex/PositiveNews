using PositiveNews.Application.DTOs;
using System.Xml.Linq;

public interface IFeedItemParser
{
    RssFeedItemDto Parse(XElement itemElement, XNamespace contentNs,
                         XNamespace DcNs, XNamespace MediaNs);
}