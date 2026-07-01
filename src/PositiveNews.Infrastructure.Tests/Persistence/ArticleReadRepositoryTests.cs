using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.Repositories.Read;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class ArticleReadRepositoryTests
{
    [Fact]
    public async Task GetFeedPageAsync_Should_ExcludeNullScores_When_MinPositivitySet()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);
        var source = Source.Create("Example Source", "https://example.com");
        ctx.Sources.Add(source);
        await ctx.SaveChangesAsync();

        ctx.ArticlesMetadata.AddRange(
            ArticleMetadata.Create(source.Id, "Scored high", "https://example.com/a", null, DateTime.UtcNow, "en", 0.8m),
            ArticleMetadata.Create(source.Id, "Scored low", "https://example.com/b", null, DateTime.UtcNow, "en", 0.3m),
            ArticleMetadata.Create(source.Id, "Unscored", "https://example.com/c", null, DateTime.UtcNow, "en"));
        await ctx.SaveChangesAsync();

        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        sourceReadRepository.GetSourceFilterItemsByIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var sut = new ArticleReadRepository(ctx, sourceReadRepository);
        var filter = new ArticleFeedFilter(1, 10, [], [], MinPositivity: 0.5m);

        var result = await sut.GetFeedPageAsync(filter, CancellationToken.None);

        result.Articles.Should().ContainSingle();
        result.Articles[0].Title.Should().Be("Scored high");
    }
}
