using System.Net;
using System.Text;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Tests.TestHelpers;

internal static class FakeFeedFactory
{
    public static XDocument MinimalRssDocument()
        => XDocument.Parse(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <item>
                  <title>Hello</title>
                  <link>https://example.com/a</link>
                  <description>Desc</description>
                  <pubDate>Mon, 01 Jan 2026 12:00:00 GMT</pubDate>
                  <guid isPermaLink="false">g1</guid>
                </item>
              </channel>
            </rss>
            """);

    public static HttpResponseMessage OkXmlResponse(XDocument doc)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(doc.ToString()));
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(ms)
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml") }
            }
        };
    }

    public static HttpResponseMessage ErrorResponse(HttpStatusCode code)
        => new(code);

    public sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response);
    }

    public sealed class CancellingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromCanceled<HttpResponseMessage>(cancellationToken);
    }
}
