using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IFeedReader
{
    Task<XDocument> ReadFeedAsync(string feedUrl, CancellationToken cancellationToken = default);
}