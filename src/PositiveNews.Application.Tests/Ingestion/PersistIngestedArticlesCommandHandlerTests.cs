using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
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
    [Fact]
    public async Task Handle_Should_ReturnZeroPersisted_When_RequestHasNoItems()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);

        var result = await handler.Handle(new PersistIngestedArticlesCommand(1, "en", []), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapDtoOntoArticleMetadata_When_ItemValid()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);
        var dto = RssFeedItemBuilder.Create(title: "My Title", link: "https://news.example.com/x", externalId: "e1");

        var result = await handler.Handle(new PersistIngestedArticlesCommand(42, "de", [dto]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        articleRepo.Received(1).Add(Arg.Is<ArticleMetadata>(m =>
            m.SourceId == 42 &&
            m.Title == "My Title" &&
            m.Url == "https://news.example.com/x" &&
            m.ExternalId == "e1" &&
            m.LanguageCode == "de"));
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PassDistinctTopicNamesCaseInsensitive_When_ChunkContainsDuplicates()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Health"] = 9 });
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);
        var dto = RssFeedItemBuilder.Create(topics: ["Health", "health"]);

        await handler.Handle(new PersistIngestedArticlesCommand(1, "en", [dto]), CancellationToken.None);

        await topicRepo.Received(1).GetTopicIdsByNamesAsync(
            Arg.Is<IReadOnlyCollection<string>>(c => c.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AttachTopicIds_When_TopicNamesResolve()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["News"] = 55 });
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);
        var dto = RssFeedItemBuilder.Create(topics: ["News"]);

        ArticleMetadata? captured = null;
        articleRepo.When(r => r.Add(Arg.Any<ArticleMetadata>())).Do(ci => captured = ci.Arg<ArticleMetadata>());

        await handler.Handle(new PersistIngestedArticlesCommand(1, "en", [dto]), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ArticleTopics.Should().HaveCount(1);
        captured.ArticleTopics.First().TopicId.Should().Be(55);
    }

    [Fact]
    public async Task Handle_Should_CallSaveChangesOncePerChunk_When_TwentySixItems()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);

        var items = Enumerable.Range(0, 26)
            .Select(i => RssFeedItemBuilder.Create(
                title: $"T{i}",
                link: $"https://example.com/{i}",
                externalId: $"e{i}"))
            .ToList();

        var result = await handler.Handle(new PersistIngestedArticlesCommand(1, "en", items), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(26);
        await uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        IngestionPipelineConstants.ArticlePersistChunkSize.Should().Be(25);
    }

    [Fact]
    public async Task Handle_Should_ReturnDomainInvariantFailure_When_ArticleViolatesDomainRules()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);
        var invalid = RssFeedItemBuilder.Create(title: " ", link: "https://x.com");

        var result = await handler.Handle(new PersistIngestedArticlesCommand(1, "en", [invalid]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ingestion.DomainInvariantViolation");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AbortEntireCommand_When_SecondItemInChunkFailsDomainRules()
    {
        var articleRepo = Substitute.For<IArticleWriteRepository>();
        var topicRepo = Substitute.For<ITopicReadRepository>();
        topicRepo.GetTopicIdsByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var logger = Substitute.For<ILogger<PersistIngestedArticlesCommandHandler>>();
        var handler = new PersistIngestedArticlesCommandHandler(articleRepo, topicRepo, uow, logger);
        var good = RssFeedItemBuilder.Create(title: "Ok", link: "https://a.com", externalId: "1");
        var bad = RssFeedItemBuilder.Create(title: "", link: "https://b.com", externalId: "2");

        var result = await handler.Handle(new PersistIngestedArticlesCommand(1, "en", [good, bad]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        articleRepo.Received(1).Add(Arg.Any<ArticleMetadata>());
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
