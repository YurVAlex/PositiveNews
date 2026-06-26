using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.Repositories.Write;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class ArticleWriteRepositoryTests
{
    [Fact]
    public async Task DeactivateBySourceAsync_Should_NotThrow_When_ArticleAlreadyInactiveInTrackedContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);
        var source = Source.Create("Example Source", "https://example.com");
        ctx.Sources.Add(source);
        await ctx.SaveChangesAsync();

        var article = ArticleMetadata.Create(
            source.Id,
            "Example article",
            "https://example.com/articles/1",
            externalId: null,
            publishedAt: DateTime.UtcNow,
            languageCode: "en");
        ctx.ArticlesMetadata.Add(article);
        await ctx.SaveChangesAsync();

        var trackedArticle = await ctx.ArticlesMetadata.SingleAsync(a => a.SourceId == source.Id);
        trackedArticle.Deactivate(99);

        var sut = new ArticleWriteRepository(ctx);

        var act = async () => await sut.DeactivateBySourceAsync(source.Id, 1, CancellationToken.None);

        await act.Should().NotThrowAsync();
        trackedArticle.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateBySourceAsync_Should_NotThrow_When_ArticleAlreadyActiveInTrackedContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);
        var source = Source.Create("Example Source", "https://example.com");
        ctx.Sources.Add(source);
        await ctx.SaveChangesAsync();

        var article = ArticleMetadata.Create(
            source.Id,
            "Example article",
            "https://example.com/articles/1",
            externalId: null,
            publishedAt: DateTime.UtcNow,
            languageCode: "en");
        ctx.ArticlesMetadata.Add(article);
        await ctx.SaveChangesAsync();

        var sut = new ArticleWriteRepository(ctx);

        var act = async () => await sut.ActivateBySourceAsync(source.Id, 1, CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await ctx.ArticlesMetadata.SingleAsync(a => a.SourceId == source.Id)).IsActive.Should().BeTrue();
    }
}
