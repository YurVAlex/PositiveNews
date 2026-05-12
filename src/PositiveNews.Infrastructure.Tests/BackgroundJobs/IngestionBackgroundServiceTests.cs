using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Infrastructure.BackgroundJobs;

namespace PositiveNews.Infrastructure.Tests.BackgroundJobs;

public class IngestionBackgroundServiceTests
{
    private sealed class TestIngestionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<IngestionBackgroundService> logger,
        IConfiguration configuration)
        : IngestionBackgroundService(scopeFactory, logger, configuration)
    {
        protected override Task DelayInitialAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        protected override Task DelayBetweenCyclesAsync(CancellationToken stoppingToken)
            => Task.Delay(Timeout.Infinite, stoppingToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_SendRunCycleCommand_When_ServiceStarts()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RunIngestionCycleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var inner = new ServiceCollection();
        inner.AddSingleton(mediator);
        var innerSp = inner.BuildServiceProvider();

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(innerSp);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Ingestion:IntervalMinutes"] = "60" }).Build();

        var sut = new TestIngestionBackgroundService(
            scopeFactory,
            NullLogger<IngestionBackgroundService>.Instance,
            config);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(150);
        await sut.StopAsync(CancellationToken.None);

        await mediator.Received(1).Send(Arg.Any<RunIngestionCycleCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogWarning_When_CycleReturnsFailure()
    {
        var mediator = Substitute.For<IMediator>();
        var err = new Error("X", "fail", ErrorType.Unexpected);
        mediator.Send(Arg.Any<RunIngestionCycleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(err)));

        var inner = new ServiceCollection();
        inner.AddSingleton(mediator);
        var innerSp = inner.BuildServiceProvider();

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(innerSp);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Ingestion:IntervalMinutes"] = "60" }).Build();

        var sut = new TestIngestionBackgroundService(
            scopeFactory,
            NullLogger<IngestionBackgroundService>.Instance,
            config);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(150);
        await sut.StopAsync(CancellationToken.None);

        await mediator.Received(1).Send(Arg.Any<RunIngestionCycleCommand>(), Arg.Any<CancellationToken>());
    }
}
