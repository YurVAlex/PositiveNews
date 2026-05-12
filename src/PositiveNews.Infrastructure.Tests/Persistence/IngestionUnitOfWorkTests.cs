using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.UnitOfWork;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class IngestionUnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_Should_DelegateToDbContext_When_NoChanges()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var sut = new IngestionUnitOfWork(ctx);

        var n = await sut.SaveChangesAsync(CancellationToken.None);

        n.Should().Be(0);
    }
}
