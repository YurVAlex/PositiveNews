using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class RunIngestionCycleCommandHandler : IRequestHandler<RunIngestionCycleCommand>
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

    public async Task Handle(RunIngestionCycleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("---=== Ingestion cycle started. ===---");

        TopicLookup topicLookup;
        IReadOnlyList<IngestionSourceSnapshot> activeSources;

        // This scope fetches the topics and active sources. Once it fetches them, it closes.
        // This ensures the database connection used for this quick read is released immediately
        using (var initialScope = _scopeFactory.CreateScope())
        {
            var mediator = initialScope.ServiceProvider.GetRequiredService<IMediator>();
            topicLookup = await mediator.Send(new GetTopicLookupQuery(), cancellationToken);
            _logger.LogInformation("Topic lookup built with {Count} topics.", topicLookup.ByName.Count);
            activeSources = await mediator.Send(new GetActiveIngestionSourcesQuery(), cancellationToken);
        }

        _logger.LogInformation("Found {Count} active sources with feed URLs.", activeSources.Count);

        foreach (var source in activeSources)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Get a brand - new, clean database connection for each news site.
            // If one fails, the others may succeed, and memory is cleared after each loop.
            using var sourceScope = _scopeFactory.CreateScope();
            var mediator = sourceScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ProcessIngestionSourceCommand(source, topicLookup), cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting {Delay} before next source...", 
                    IngestionPipelineConstants.DelayBetweenSources);
                await Task.Delay(IngestionPipelineConstants.DelayBetweenSources, cancellationToken);
            }
        }

        _logger.LogInformation("=== Ingestion cycle completed. ===");
    }
}