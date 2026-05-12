using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Retrieves RSS or Atom XML from a remote feed URL.
/// </summary>
public interface IFeedReader
{
    /// <summary>
    /// Downloads and parses the feed document from the given URL.
    /// </summary>
    /// <param name="feedUrl">HTTP(S) location of the RSS feed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed XML document.</returns>
    Task<XDocument> ReadFeedAsync(string feedUrl, CancellationToken cancellationToken = default);
}
