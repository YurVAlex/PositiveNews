using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.CommandHandlers.Ingestion;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Tests.Ingestion;

public class TriggerIngestionCycleCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CycleAlreadyRunning()
    {
        var coordinator = Substitute.For<IIngestionCycleCoordinator>();
        coordinator.IsRunning.Returns(true);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var handler = new TriggerIngestionCycleCommandHandler(
            coordinator,
            scopeFactory,
            NullLogger<TriggerIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new TriggerIngestionCycleCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Ingestion.AlreadyRunning);
        scopeFactory.DidNotReceive().CreateScope();
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_NotRunning()
    {
        var coordinator = Substitute.For<IIngestionCycleCoordinator>();
        coordinator.IsRunning.Returns(false);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var handler = new TriggerIngestionCycleCommandHandler(
            coordinator,
            scopeFactory,
            NullLogger<TriggerIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new TriggerIngestionCycleCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
