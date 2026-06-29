using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class IngestionArticleBatchSaverTests
{
    [Fact]
    public void IsUniqueConstraintViolation_Should_ReturnFalse_When_InnerExceptionIsNotSqlUniqueViolation()
    {
        var dbEx = new DbUpdateException("other", new InvalidOperationException("db"));

        IngestionArticleBatchSaver.IsUniqueConstraintViolation(dbEx).Should().BeFalse();
    }

    [Fact]
    public async Task SaveAddedArticlesAsync_Should_ReturnArticleCount_When_BatchSaveSucceeds()
    {
        var uow = Substitute.For<IIngestionUnitOfWork>();
        var repo = Substitute.For<IArticleWriteRepository>();
        var sut = new IngestionArticleBatchSaver(uow, repo, NullLogger<IngestionArticleBatchSaver>.Instance);
        var articles = new List<ArticleMetadata>
        {
            ArticleMetadata.Create(1, "A", "https://example.com/a", "e1", DateTime.UtcNow, "en")
        };

        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var saved = await sut.SaveAddedArticlesAsync(articles, CancellationToken.None);

        saved.Should().Be(1);
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        repo.DidNotReceive().Add(Arg.Any<ArticleMetadata>());
    }
}
