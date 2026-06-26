using FluentAssertions;
using PositiveNews.Infrastructure.Ingestion;

namespace PositiveNews.Infrastructure.Tests.Ingestion;

public class IngestionCycleCoordinatorTests
{
    [Fact]
    public void TryBeginCycle_Should_ReturnFalse_When_AlreadyRunning()
    {
        var sut = new IngestionCycleCoordinator();

        sut.TryBeginCycle().Should().BeTrue();
        sut.IsRunning.Should().BeTrue();
        sut.TryBeginCycle().Should().BeFalse();
    }

    [Fact]
    public void EndCycle_Should_AllowNewCycle_When_CalledAfterBegin()
    {
        var sut = new IngestionCycleCoordinator();

        sut.TryBeginCycle().Should().BeTrue();
        sut.EndCycle();
        sut.IsRunning.Should().BeFalse();
        sut.TryBeginCycle().Should().BeTrue();
    }

    [Fact]
    public void SetNextRunAtUtc_Should_BeReadable()
    {
        var sut = new IngestionCycleCoordinator();
        var at = new DateTime(2026, 5, 22, 12, 0, 0, DateTimeKind.Utc);

        sut.SetNextRunAtUtc(at);

        sut.NextRunAtUtc.Should().Be(at);
    }
}
