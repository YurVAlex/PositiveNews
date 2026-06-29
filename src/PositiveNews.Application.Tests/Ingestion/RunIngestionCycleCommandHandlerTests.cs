using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PositiveNews.Application.CommandHandlers.Ingestion;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Application.Tests.TestSupport;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.Tests.Ingestion;

public class RunIngestionCycleCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_SendRefreshLookupAndSources_When_InitialScopeOpens()
    {
        var settings = IngestionTestData.MinimalSettings();
        var lookup = IngestionTestData.EmptyTopicLookup();
        var sources = new List<IngestionSourceSnapshot> { IngestionTestData.ValidSource() };
        var initialMediator = Substitute.For<IMediator>();
        var processMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>()).Returns(settings);
        initialMediator.Send(Arg.Any<GetTopicLookupQuery>(), Arg.Any<CancellationToken>()).Returns(lookup);
        initialMediator.Send(Arg.Any<GetActiveIngestionSourcesQuery>(), Arg.Any<CancellationToken>()).Returns(sources);
        var cts = new CancellationTokenSource();
        processMediator.Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                cts.Cancel();
                return Task.FromResult(Result<int>.Success(1));
            });
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            new AllowingIngestionCycleCoordinator(),
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), cts.Token);

        result.IsSuccess.Should().BeTrue();
        await initialMediator.Received(1).Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>());
        await initialMediator.Received(1).Send(Arg.Any<GetTopicLookupQuery>(), Arg.Any<CancellationToken>());
        await initialMediator.Received(1).Send(Arg.Any<GetActiveIngestionSourcesQuery>(), Arg.Any<CancellationToken>());
        await processMediator.Received(1).Send(
            Arg.Is<ProcessIngestionSourceCommand>(c =>
                ReferenceEquals(c.TopicLookup, lookup) && ReferenceEquals(c.IngestionSettings, settings)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_AnySourceProcessingFails()
    {
        var settings = IngestionTestData.MinimalSettings();
        var lookup = IngestionTestData.EmptyTopicLookup();
        var sources = new List<IngestionSourceSnapshot>
        {
            IngestionTestData.ValidSource(1),
            IngestionTestData.ValidSource(2)
        };
        var initialMediator = Substitute.For<IMediator>();
        var processMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>()).Returns(settings);
        initialMediator.Send(Arg.Any<GetTopicLookupQuery>(), Arg.Any<CancellationToken>()).Returns(lookup);
        initialMediator.Send(Arg.Any<GetActiveIngestionSourcesQuery>(), Arg.Any<CancellationToken>()).Returns(sources);
        var err = new Error("Ingestion.Failed", "stopped", ErrorType.Unexpected);
        processMediator.Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Failure(err));
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            new AllowingIngestionCycleCoordinator(),
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(err);
        await processMediator.Received(1).Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotProcessSecondSource_When_CancelledAfterFirstSource()
    {
        var settings = IngestionTestData.MinimalSettings();
        var lookup = IngestionTestData.EmptyTopicLookup();
        var sources = new List<IngestionSourceSnapshot>
        {
            IngestionTestData.ValidSource(1),
            IngestionTestData.ValidSource(2)
        };
        var initialMediator = Substitute.For<IMediator>();
        var processMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>()).Returns(settings);
        initialMediator.Send(Arg.Any<GetTopicLookupQuery>(), Arg.Any<CancellationToken>()).Returns(lookup);
        initialMediator.Send(Arg.Any<GetActiveIngestionSourcesQuery>(), Arg.Any<CancellationToken>()).Returns(sources);
        var cts = new CancellationTokenSource();
        processMediator.Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                cts.Cancel();
                return Task.FromResult(Result<int>.Success(1));
            });
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            new AllowingIngestionCycleCoordinator(),
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), cts.Token);

        result.IsSuccess.Should().BeTrue();
        await processMediator.Received(1).Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_DomainExceptionThrownDuringInitialPhase()
    {
        var initialMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidUserStateException("settings corrupt"));
        var processMediator = Substitute.For<IMediator>();
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            new AllowingIngestionCycleCoordinator(),
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ingestion.DomainInvariantViolation");
        await processMediator.DidNotReceive().Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UnexpectedExceptionThrownDuringInitialPhase()
    {
        var initialMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("settings corrupt"));
        var processMediator = Substitute.For<IMediator>();
        var coordinator = new AllowingIngestionCycleCoordinator();
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            coordinator,
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ingestion.Unexpected");
        coordinator.IsRunning.Should().BeFalse();
        await processMediator.DidNotReceive().Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ProcessEveryActiveSource_When_AllSucceed()
    {
        var settings = IngestionTestData.MinimalSettings();
        var lookup = IngestionTestData.EmptyTopicLookup();
        var sources = new List<IngestionSourceSnapshot>
        {
            IngestionTestData.ValidSource(10),
            IngestionTestData.ValidSource(11)
        };
        var initialMediator = Substitute.For<IMediator>();
        var processMediator = Substitute.For<IMediator>();
        initialMediator.Send(Arg.Any<RefreshIngestionSettingsCommand>(), Arg.Any<CancellationToken>()).Returns(settings);
        initialMediator.Send(Arg.Any<GetTopicLookupQuery>(), Arg.Any<CancellationToken>()).Returns(lookup);
        initialMediator.Send(Arg.Any<GetActiveIngestionSourcesQuery>(), Arg.Any<CancellationToken>()).Returns(sources);
        processMediator.Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(2)));
        var factory = new TestServiceScopeFactory(
            new TestServiceScope(new TestServiceProvider(initialMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)),
            new TestServiceScope(new TestServiceProvider(processMediator)));
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            new AllowingIngestionCycleCoordinator(),
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await processMediator.Received(2).Send(Arg.Any<ProcessIngestionSourceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CycleAlreadyRunning()
    {
        var coordinator = new AllowingIngestionCycleCoordinator();
        coordinator.TryBeginCycle();
        var factory = Substitute.For<IServiceScopeFactory>();
        var handler = new RunIngestionCycleCommandHandler(
            factory,
            coordinator,
            NullLogger<RunIngestionCycleCommandHandler>.Instance);

        var result = await handler.Handle(new RunIngestionCycleCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ingestion.AlreadyRunning");
        factory.DidNotReceive().CreateScope();
    }
}
