using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.CommandHandlers.Admin;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Admin;

public class ModerateArticleCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleMissing()
    {
        var articleWriteRepository = Substitute.For<IArticleWriteRepository>();
        articleWriteRepository.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns((ArticleMetadata?)null);
        var handler = new ModerateArticleCommandHandler(
            articleWriteRepository,
            Substitute.For<IAuditLogWriteRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(
            new ModerateArticleCommand(
                123,
                true,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                1),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.ArticleNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_UpdateMetadataAndLogChangedFields_When_FieldsDiffer()
    {
        var article = ArticleMetadata.Create(
            sourceId: 5,
            title: "Old title",
            url: "https://example.com/article",
            externalId: "ext-1",
            publishedAt: DateTime.UtcNow.AddDays(-1),
            languageCode: "en",
            positivityScore: 0.25m,
            author: "Author",
            summaryShort: "Short summary",
            imageTag: "<img src=\"old.jpg\" />");
        typeof(ArticleMetadata).GetProperty(nameof(ArticleMetadata.Id))!.SetValue(article, 10L);
        article.AttachContent(ArticleContent.Create("<p>Old body</p>", "<p>Old body</p>"));

        var articleWriteRepository = Substitute.For<IArticleWriteRepository>();
        articleWriteRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(article);
        var auditLogWriteRepository = Substitute.For<IAuditLogWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ModerateArticleCommandHandler(articleWriteRepository, auditLogWriteRepository, unitOfWork);

        var result = await handler.Handle(
            new ModerateArticleCommand(
                10,
                true,
                "New title",
                "<img src=\"new.jpg\" />",
                0.75m,
                "Short summary",
                "<p>New body</p>",
                "moderation-reason",
                "moderation-note",
                42),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        article.Title.Should().Be("New title");
        article.ImageTag.Should().Be("<img src=\"new.jpg\" />");
        article.PositivityScore.Should().Be(0.75m);
        article.Content.Should().NotBeNull();
        article.Content!.ContentRaw.Should().Be("<p>New body</p>");

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(ArticleMetadata.Title)
            && log.OldValue == "Old title"
            && log.NewValue == "New title"
            && log.Reason == "moderation-reason"
            && log.Note == "moderation-note"));
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(ArticleMetadata.ImageTag)
            && log.OldValue == "<img src=\"old.jpg\" />"
            && log.NewValue == "<img src=\"new.jpg\" />"));
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(ArticleMetadata.PositivityScore)
            && log.OldValue == 0.25m.ToString()
            && log.NewValue == 0.75m.ToString()));
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(ArticleContent.ContentRaw)
            && log.OldValue == "<p>Old body</p>"
            && log.NewValue == "<p>New body</p>"));
        article.ModeratedBy.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_SetModeratedBy_When_MetadataChangesAndActiveStateUnchanged()
    {
        var article = ArticleMetadata.Create(
            sourceId: 5,
            title: "Old title",
            url: "https://example.com/article",
            externalId: "ext-1",
            publishedAt: DateTime.UtcNow.AddDays(-1),
            languageCode: "en",
            positivityScore: 0.25m,
            author: "Author",
            summaryShort: "Short summary",
            imageTag: "<img src=\"old.jpg\" />");
        typeof(ArticleMetadata).GetProperty(nameof(ArticleMetadata.Id))!.SetValue(article, 10L);
        article.AttachContent(ArticleContent.Create("<p>Old body</p>", "<p>Old body</p>"));

        var articleWriteRepository = Substitute.For<IArticleWriteRepository>();
        articleWriteRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(article);
        var auditLogWriteRepository = Substitute.For<IAuditLogWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ModerateArticleCommandHandler(articleWriteRepository, auditLogWriteRepository, unitOfWork);

        var result = await handler.Handle(
            new ModerateArticleCommand(
                10,
                true,
                "New title",
                null,
                null,
                null,
                null,
                "moderation-reason",
                "moderation-note",
                42),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        article.ModeratedBy.Should().Be(42);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationFailure_When_NoChangesProvided()
    {
        var article = ArticleMetadata.Create(
            sourceId: 5,
            title: "Same title",
            url: "https://example.com/article",
            externalId: "ext-1",
            publishedAt: DateTime.UtcNow.AddDays(-1),
            languageCode: "en",
            positivityScore: 0.5m,
            author: "Author",
            summaryShort: "Same summary",
            imageTag: "<img src=\"same.jpg\" />");
        typeof(ArticleMetadata).GetProperty(nameof(ArticleMetadata.Id))!.SetValue(article, 20L);
        article.AttachContent(ArticleContent.Create("<p>Same body</p>", "<p>Same body</p>"));

        var articleWriteRepository = Substitute.For<IArticleWriteRepository>();
        articleWriteRepository.GetByIdAsync(20, Arg.Any<CancellationToken>()).Returns(article);
        var auditLogWriteRepository = Substitute.For<IAuditLogWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ModerateArticleCommandHandler(articleWriteRepository, auditLogWriteRepository, unitOfWork);

        var result = await handler.Handle(
            new ModerateArticleCommand(
                20,
                true,
                "Same title",
                "<img src=\"same.jpg\" />",
                0.5m,
                "Same summary",
                "<p>Same body</p>",
                "no-op",
                "no-op",
                99),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.ArticleUnchanged");
        result.Error.Type.Should().Be(ErrorType.Validation);
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        auditLogWriteRepository.DidNotReceive().Add(Arg.Any<AuditLog>());
    }
}
