using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.UnitOfWork;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_Should_DelegateToDbContext_When_NoChanges()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var sut = new UnitOfWork(ctx);

        var n = await sut.SaveChangesAsync(CancellationToken.None);

        n.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_ReturnPositiveCount_When_EntityAdded()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var sut = new UnitOfWork(ctx);
        ctx.Roles.Add(Role.Create("TestRole"));

        var n = await sut.SaveChangesAsync(CancellationToken.None);

        n.Should().Be(1);
    }
}
