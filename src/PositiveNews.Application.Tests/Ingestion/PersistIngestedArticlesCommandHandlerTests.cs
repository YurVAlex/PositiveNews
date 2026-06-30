using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.CommandHandlers.Ingestion;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Tests.TestSupport;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Ingestion;

public class PersistIngestedArticlesCommandHandlerTests
{
    private static PersistIngestedArticlesCommandHandler CreateHandler(
        IArticleWriteRepository articleRepo,
        IIngestionArticleBatchSaver? batchSaver = null,
        ILogger<PersistIngestedArticlesCommandHandler>? logger = null)
    {
        batchSaver ??= CreateDefaultBatchSaver();
        logger ??= Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        return new PersistIngestedArticlesCommandHandler(articleRepo, batchSaver, logger);
    }

    private static IIngestionArticleBatchSaver CreateDefaultBatchSaver()
    {
        var batchSaver = Substitute.For<IIngestionArticleBatchSaver>();
        batchSaver.SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<IReadOnlyList<ArticleMetadata>>().Count));
        return batchSaver;
    }

    [Fact]
    public async Task Handle_Should_ReturnZeroPersisted_When_RequestHasNoItems()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = Substitute.For<IIngestionArticleBatchSaver>();
        var handler = CreateHandler(articleRepo, batchSaver);

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", IngestionTestData.EmptyTopicLookup(), []),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await batchSaver.DidNotReceive().SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapDtoOntoArticleMetadata_When_ItemValid()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = CreateDefaultBatchSaver();
        var handler = CreateHandler(articleRepo, batchSaver);
        var dto = RssFeedItemBuilder.Create(title: "My Title", link: "https://news.example.com/x", externalId: "e1");

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(42, "de", IngestionTestData.EmptyTopicLookup(), [dto]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        articleRepo.Received(1).Add(Arg.Is<ArticleMetadata>(m =>
            m.SourceId == 42 &&
            m.Title == "My Title" &&
            m.Url == "https://news.example.com/x" &&
            m.ExternalId == "e1" &&
            m.LanguageCode == "de"));
        await batchSaver.Received(1).SaveAddedArticlesAsync(
            Arg.Is<IReadOnlyList<ArticleMetadata>>(list => list.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AttachSingleTopic_When_DuplicateTopicNamesDifferOnlyByCase()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var handler = CreateHandler(articleRepo);
        var lookup = IngestionTestData.TopicLookupWith(("Health", 9));
        var dto = RssFeedItemBuilder.Create(topics: ["Health", "health"]);

        ArticleMetadata? captured = null;
        articleRepo.When(r => r.Add(Arg.Any<ArticleMetadata>())).Do(ci => captured = ci.Arg<ArticleMetadata>());

        await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", lookup, [dto]),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ArticleTopics.Should().HaveCount(1);
        captured.ArticleTopics.First().TopicId.Should().Be(9);
    }

    [Fact]
    public async Task Handle_Should_AttachTopicIds_When_TopicNamesResolve()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var handler = CreateHandler(articleRepo);
        var lookup = IngestionTestData.TopicLookupWith(("News", 55));
        var dto = RssFeedItemBuilder.Create(topics: ["News"]);

        ArticleMetadata? captured = null;
        articleRepo.When(r => r.Add(Arg.Any<ArticleMetadata>())).Do(ci => captured = ci.Arg<ArticleMetadata>());

        await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", lookup, [dto]),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ArticleTopics.Should().HaveCount(1);
        captured.ArticleTopics.First().TopicId.Should().Be(55);
    }

    [Fact]
    public async Task Handle_Should_CallBatchSaverOncePerChunk_When_TwentySixItems()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = CreateDefaultBatchSaver();
        var handler = CreateHandler(articleRepo, batchSaver);

        var items = Enumerable.Range(0, 26)
            .Select(i => RssFeedItemBuilder.Create(
                title: $"T{i}",
                link: $"https://example.com/{i}",
                externalId: $"e{i}"))
            .ToList();

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", IngestionTestData.EmptyTopicLookup(), items),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(26);
        await batchSaver.Received(2).SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>());
        IngestionPipelineConstants.ArticlePersistChunkSize.Should().Be(25);
    }

    [Fact]
    public async Task Handle_Should_ReturnDomainInvariantFailure_When_ArticleViolatesDomainRules()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = Substitute.For<IIngestionArticleBatchSaver>();
        var handler = CreateHandler(articleRepo, batchSaver);
        var invalid = RssFeedItemBuilder.Create(title: " ", link: "https://x.com");

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", IngestionTestData.EmptyTopicLookup(), [invalid]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Ingestion.DomainInvariantViolation);
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await batchSaver.DidNotReceive().SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AbortEntireCommand_When_SecondItemInChunkFailsDomainRules()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = Substitute.For<IIngestionArticleBatchSaver>();
        var handler = CreateHandler(articleRepo, batchSaver);
        var good = RssFeedItemBuilder.Create(title: "Ok", link: "https://a.com", externalId: "1");
        var bad = RssFeedItemBuilder.Create(title: "", link: "https://b.com", externalId: "2");

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", IngestionTestData.EmptyTopicLookup(), [good, bad]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        articleRepo.Received(1).Add(Arg.Any<ArticleMetadata>());
        await batchSaver.DidNotReceive().SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnPartialCount_When_BatchSaverSkipsDuplicates()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var batchSaver = Substitute.For<IIngestionArticleBatchSaver>();
        batchSaver.SaveAddedArticlesAsync(Arg.Any<IReadOnlyList<ArticleMetadata>>(), Arg.Any<CancellationToken>())
            .Returns(1);
        var handler = CreateHandler(articleRepo, batchSaver);
        var items = new[]
        {
            RssFeedItemBuilder.Create(title: "A", link: "https://a.com", externalId: "dup"),
            RssFeedItemBuilder.Create(title: "B", link: "https://b.com", externalId: "fresh")
        };

        var result = await handler.Handle(
            new PersistIngestedArticlesCommand(1, "en", IngestionTestData.EmptyTopicLookup(), items),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }
}
