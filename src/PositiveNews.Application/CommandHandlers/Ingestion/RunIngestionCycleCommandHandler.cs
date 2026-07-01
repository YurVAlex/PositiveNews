using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Constants;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

/// <summary>
/// Executes the multi-source ingestion cycle with isolated scopes per source and configurable delays between sources.
/// </summary>
public sealed class RunIngestionCycleCommandHandler : IRequestHandler<RunIngestionCycleCommand, Result>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIngestionCycleCoordinator _coordinator;
    private readonly ILogger<RunIngestionCycleCommandHandler> _logger;

    /// <summary>
    /// Initializes the handler with scope creation and logging dependencies.
    /// </summary>
    /// <param name="scopeFactory">Creates scoped mediators for each processing stage.</param>
    /// <param name="coordinator">Tracks cycle execution for concurrency and admin status.</param>
    /// <param name="logger">Logs cycle milestones.</param>
    public RunIngestionCycleCommandHandler(
        IServiceScopeFactory scopeFactory,
        IIngestionCycleCoordinator coordinator,
        ILogger<RunIngestionCycleCommandHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _coordinator = coordinator;
        _logger = logger;
    }

    /// <summary>
    /// Refreshes settings, builds topic lookup, loads active sources, processes each source sequentially, and stops on hard failures.
    /// </summary>
    /// <param name="request">Marker command.</param>
    /// <param name="cancellationToken">Cancellation token observed between sources.</param>
    /// <returns>Success when every source completes without fatal errors.</returns>
    public async Task<Result> Handle(RunIngestionCycleCommand request, CancellationToken cancellationToken)
    {
        if (!_coordinator.TryBeginCycle())
        {
            return Result.Failure(
                new Error(ErrorCodes.Ingestion.AlreadyRunning, "An ingestion cycle is already in progress.", ErrorType.Conflict));
        }

        try
        {
            _logger.LogInformation("---=== Ingestion cycle started. ===---");

            TopicLookup topicLookup;
            IReadOnlyList<IngestionSourceSnapshot> activeSources;
            IngestionSettingsSnapshot ingestionSettings;

            using (var initialScope = _scopeFactory.CreateScope())
            {
                var mediator = initialScope.ServiceProvider.GetRequiredService<IMediator>();

                ingestionSettings = await mediator.Send(new RefreshIngestionSettingsCommand(), cancellationToken);

                topicLookup = await mediator.Send(new GetTopicLookupQuery(), cancellationToken);
                _logger.LogInformation("Topic lookup built with {Count} topics.", topicLookup.ByName.Count);
                activeSources = await mediator.Send(new GetActiveIngestionSourcesQuery(), cancellationToken);
            }

            _logger.LogInformation("Found {Count} active sources with feed URLs.", activeSources.Count);

            foreach (var source in activeSources)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                using var sourceScope = _scopeFactory.CreateScope();
                var mediator = sourceScope.ServiceProvider.GetRequiredService<IMediator>();
                var processResult = await mediator.Send(
                    new ProcessIngestionSourceCommand(source, topicLookup, ingestionSettings),
                    cancellationToken);

                if (processResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Stopping ingestion cycle due to source failure: {ErrorCode} - {ErrorMessage}",
                        processResult.Error.Code,
                        processResult.Error.Message);
                    return Result.Failure(processResult.Error);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug(
                        "Waiting {Delay} before next source...",
                        IngestionPipelineConstants.DelayBetweenSources);
                    await Task.Delay(IngestionPipelineConstants.DelayBetweenSources, cancellationToken);
                }
            }

            _logger.LogInformation("=== Ingestion cycle completed. ===");
            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain invariant violation while running ingestion cycle.");
            return Result.Failure(
                new Error(ErrorCodes.Ingestion.DomainInvariantViolation, ex.Message, ErrorType.Conflict));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while running ingestion cycle.");
            return Result.Failure(
                new Error(ErrorCodes.Ingestion.Unexpected, ex.Message, ErrorType.Unexpected));
        }
        finally
        {
            _coordinator.EndCycle();
        }
    }
}
