using System.IO;
using System.Xml.Linq;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.CommandHandlers.Ingestion;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Application.Services.Ingestion;
using PositiveNews.Application.Tests.TestSupport;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.Tests.Ingestion;

public class ProcessIngestionSourceCommandHandlerTests
{
    private static ProcessIngestionSourceCommandHandler CreateHandler(
        IIngestionRunRepository runRepo,
        IIngestionUnitOfWork uow,
        IArticleDeduplicator dedup,
        IFeedReader reader,
        IFeedProcessor processor,
        IMediator mediator)
    {
        var logger = Substitute.For<ILogger<ProcessIngestionSourceCommandHandler>>();
        return new ProcessIngestionSourceCommandHandler(runRepo, uow, dedup, reader, processor, mediator, logger);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessWithZero_When_FeedContainsNoItems()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = Substitute.For<IArticleDeduplicator>();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(
                Arg.Any<string>(),
                Arg.Any<XDocument>(),
                Arg.Any<TopicLookup>(),
                Arg.Any<IngestionSettingsSnapshot>(),
                Arg.Any<IngestionSourceSnapshot>(),
                Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult([], 0));
        IngestionRun? run = null;
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => run = ci.Arg<IngestionRun>());
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        var result = await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        run.Should().NotBeNull();
        run!.Status.Should().Be(IngestionStatus.Partial);
        await uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SendFindExistingKeysAndPersist_When_ItemsNeedSaving()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        var itemA = RssFeedItemBuilder.Create(title: "A", link: "https://a.com", externalId: "e1");
        var itemB = RssFeedItemBuilder.Create(title: "B", link: "https://b.com", externalId: "e2");
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult([itemA, itemB], 0));
        mediator.Send(Arg.Any<FindExistingArticleKeysQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingArticleKeys(new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));
        mediator.Send(Arg.Any<PersistIngestedArticlesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(2));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        var source = IngestionTestData.ValidSource();
        var result = await handler.Handle(
            new ProcessIngestionSourceCommand(source, IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        await mediator.Received(1).Send(
            Arg.Is<FindExistingArticleKeysQuery>(q =>
                q.ExternalIds.SequenceEqual(new[] { "e1", "e2" }) &&
                q.Urls.SequenceEqual(new[] { "https://a.com", "https://b.com" }) &&
                q.Titles.SequenceEqual(new[] { "A", "B" })),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<PersistIngestedArticlesCommand>(p =>
                p.SourceId == source.Id &&
                p.DefaultLanguageCode == source.DefaultLanguageCode &&
                p.Items.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SkipDuplicatesAgainstExistingKeys_When_ArticleAlreadyStored()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        var existingUrl = "https://existing.com/a";
        var dup = RssFeedItemBuilder.Create(title: "Dup", link: existingUrl, externalId: "x");
        var fresh = RssFeedItemBuilder.Create(title: "Fresh", link: "https://new.com/b", externalId: "y");
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult([dup, fresh], 0));
        mediator.Send(Arg.Any<FindExistingArticleKeysQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingArticleKeys(new HashSet<string>(), new HashSet<string>([existingUrl]), new HashSet<string>()));
        mediator.Send(Arg.Any<PersistIngestedArticlesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<PersistIngestedArticlesCommand>(p => p.Items.Count == 1 && p.Items[0].Link == fresh.Link),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_FailRunAndPropagateError_When_PersistReturnsFailure()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        var item = RssFeedItemBuilder.Create();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult([item], 0));
        mediator.Send(Arg.Any<FindExistingArticleKeysQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingArticleKeys(new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));
        var persistErr = new Error("Ingestion.SaveFailed", "db down", ErrorType.Unexpected);
        mediator.Send(Arg.Any<PersistIngestedArticlesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Failure(persistErr));
        IngestionRun? run = null;
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => run = ci.Arg<IngestionRun>());
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        var result = await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(persistErr);
        run.Should().NotBeNull();
        run!.Status.Should().Be(IngestionStatus.Failed);
    }

    [Fact]
    public async Task Handle_Should_MapDomainExceptionToFailure_When_ProcessFeedThrowsDomainException()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = Substitute.For<IArticleDeduplicator>();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidArticleStateException("bad feed"));
        IngestionRun? run = null;
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => run = ci.Arg<IngestionRun>());
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        var result = await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ingestion.DomainInvariantViolation");
        run!.Status.Should().Be(IngestionStatus.Failed);
    }

    [Fact]
    public async Task Handle_Should_RethrowAndMarkPartial_When_CancellationRequestedMidBatch()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        var items = Enumerable.Range(0, 3).Select(i => RssFeedItemBuilder.Create(title: $"T{i}", link: $"https://x.com/{i}", externalId: $"{i}")).ToList();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult(items, 0));
        mediator.Send(Arg.Any<FindExistingArticleKeysQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingArticleKeys(new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));
        var cts = new CancellationTokenSource();
        mediator.Send(Arg.Any<PersistIngestedArticlesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        IngestionRun? run = null;
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => run = ci.Arg<IngestionRun>());
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            cts.Token));

        run!.Status.Should().Be(IngestionStatus.Partial);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessWithPartialCount_When_OperationCanceledWithoutCancellationRequested()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new OperationCanceledException("timeout"));
        IngestionRun? run = null;
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => run = ci.Arg<IngestionRun>());
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        var result = await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        run!.Status.Should().Be(IngestionStatus.Partial);
    }

    [Fact]
    public async Task Handle_Should_PropagateIOException_When_FeedReaderThrows()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = Substitute.For<IArticleDeduplicator>();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<XDocument>(new IOException("network")));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        await Assert.ThrowsAsync<IOException>(() => handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_PropagateGenericException_When_ProcessFeedThrowsNonDomain()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = Substitute.For<IArticleDeduplicator>();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("parse bug"));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_CallDeduplicatorForEachItem_When_FeedHasMultipleEntries()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = Substitute.For<IArticleDeduplicator>();
        dedup.MatchesExisting(Arg.Any<ExistingArticleKeys>(), Arg.Any<RssFeedItemDto>()).Returns(false);
        dedup.ConflictsWithPending(Arg.Any<RssFeedItemDto>(), Arg.Any<HashSet<string>>(), Arg.Any<HashSet<string>>(), Arg.Any<HashSet<string>>())
            .Returns(false);
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        var items = new[]
        {
            RssFeedItemBuilder.Create(title: "A", link: "https://a.com", externalId: "1"),
            RssFeedItemBuilder.Create(title: "B", link: "https://b.com", externalId: "2")
        };
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult(items, 0));
        mediator.Send(Arg.Any<FindExistingArticleKeysQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingArticleKeys(new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));
        mediator.Send(Arg.Any<PersistIngestedArticlesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(2));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);

        await handler.Handle(
            new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        dedup.Received(2).MatchesExisting(Arg.Any<ExistingArticleKeys>(), Arg.Any<RssFeedItemDto>());
    }

    [Fact]
    public async Task Handle_Should_CreateIngestionRun_When_HandlerStarts()
    {
        var runRepo = Substitute.For<IIngestionRunRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var dedup = new ArticleDeduplicator();
        var reader = Substitute.For<IFeedReader>();
        var processor = Substitute.For<IFeedProcessor>();
        var mediator = Substitute.For<IMediator>();
        reader.ReadFeedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new XDocument(new XElement("rss")));
        processor.ProcessFeed(Arg.Any<string>(), Arg.Any<XDocument>(), Arg.Any<TopicLookup>(), Arg.Any<IngestionSettingsSnapshot>(), Arg.Any<IngestionSourceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(new FeedProcessingResult([], 0));
        var statusesAtAdd = new List<IngestionStatus>();
        runRepo.When(r => r.Add(Arg.Any<IngestionRun>())).Do(ci => statusesAtAdd.Add(ci.Arg<IngestionRun>().Status));
        var handler = CreateHandler(runRepo, uow, dedup, reader, processor, mediator);
        var source = IngestionTestData.ValidSource(99);

        await handler.Handle(
            new ProcessIngestionSourceCommand(source, IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings()),
            CancellationToken.None);

        statusesAtAdd.Should().ContainSingle().Which.Should().Be(IngestionStatus.Running);
        runRepo.Received(1).Add(Arg.Is<IngestionRun>(r => r.SourceId == 99));
    }
}
