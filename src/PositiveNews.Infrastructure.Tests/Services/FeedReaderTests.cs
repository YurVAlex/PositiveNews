using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Infrastructure.Services;
using PositiveNews.Infrastructure.Tests.TestHelpers;

namespace PositiveNews.Infrastructure.Tests.Services;

public class FeedReaderTests
{
    [Fact]
    public async Task ReadFeedAsync_Should_ReturnXmlDocument_When_ResponseSuccessful()
    {
        var doc = FakeFeedFactory.MinimalRssDocument();
        var handler = new FakeFeedFactory.StubHttpMessageHandler(FakeFeedFactory.OkXmlResponse(doc));
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("RssFeedClient").Returns(httpClient);
        IFeedReader sut = new FeedReader(factory, NullLogger<FeedReader>.Instance);

        var result = await sut.ReadFeedAsync("https://example.com/feed.xml", CancellationToken.None);

        result.Root.Should().NotBeNull();
        result.Root!.Name.LocalName.Should().Be("rss");
    }

    [Fact]
    public async Task ReadFeedAsync_Should_ThrowHttpRequestException_When_StatusNotSuccess()
    {
        var handler = new FakeFeedFactory.StubHttpMessageHandler(FakeFeedFactory.ErrorResponse(System.Net.HttpStatusCode.NotFound));
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("RssFeedClient").Returns(new HttpClient(handler));
        IFeedReader sut = new FeedReader(factory, NullLogger<FeedReader>.Instance);

        var act = async () => await sut.ReadFeedAsync("https://example.com/x", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ReadFeedAsync_Should_Throw_When_XmlMalformed()
    {
        var bad = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("not xml {{{")
        };
        var handler = new FakeFeedFactory.StubHttpMessageHandler(bad);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("RssFeedClient").Returns(new HttpClient(handler));
        IFeedReader sut = new FeedReader(factory, NullLogger<FeedReader>.Instance);

        var act = async () => await sut.ReadFeedAsync("https://example.com/x", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ReadFeedAsync_Should_PropagateCancellation_When_TokenCancelled()
    {
        var handler = new FakeFeedFactory.CancellingHttpMessageHandler();
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("RssFeedClient").Returns(new HttpClient(handler));
        IFeedReader sut = new FeedReader(factory, NullLogger<FeedReader>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await sut.ReadFeedAsync("https://example.com/x", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
