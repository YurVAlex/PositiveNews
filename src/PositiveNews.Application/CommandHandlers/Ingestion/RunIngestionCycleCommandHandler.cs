using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class RunIngestionCycleCommandHandler : IRequestHandler<RunIngestionCycleCommand, Result>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RunIngestionCycleCommandHandler> _logger;

    public RunIngestionCycleCommandHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<RunIngestionCycleCommandHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<Result> Handle(RunIngestionCycleCommand request, CancellationToken cancellationToken)
    {
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
                new Error("Ingestion.DomainInvariantViolation", ex.Message, ErrorType.Conflict));
        }
    }
}