using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

/// <summary>
/// Starts an ingestion cycle on a background thread when not already running.
/// </summary>
public sealed class TriggerIngestionCycleCommandHandler(
    IIngestionCycleCoordinator coordinator,
    IServiceScopeFactory scopeFactory,
    ILogger<TriggerIngestionCycleCommandHandler> logger)
    : IRequestHandler<TriggerIngestionCycleCommand, Result>
    // Can't inject MediatR in class constructor because they have different lifecycle!
{
    /// <inheritdoc />
    public Task<Result> Handle(TriggerIngestionCycleCommand request, CancellationToken cancellationToken)
    {
        if (coordinator.IsRunning)
        {
            return Task.FromResult(Result.Failure(
                new Error(ErrorCodes.Ingestion.AlreadyRunning, "An ingestion cycle is already in progress.", ErrorType.Conflict)));
        }

        Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                // Resolve MediatR from the scoped service provider to ensure it has the correct lifecycle.
                // This is because user able to run backround task from request and it should work when request lifetime ends.
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new RunIngestionCycleCommand(), CancellationToken.None);
                // Not want the manual ingestion cycle to stop because the trigger request ended. 
                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Manual ingestion cycle failed: {ErrorCode} - {ErrorMessage}",
                        result.Error.Code,
                        result.Error.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception during manual ingestion cycle.");
            }
        });

        return Task.FromResult(Result.Success());
    }
}
