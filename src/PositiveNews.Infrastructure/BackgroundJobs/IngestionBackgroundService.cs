using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Ingestion;
using PositiveNews.Application.Commands.Ingestion;

namespace PositiveNews.Infrastructure.BackgroundJobs;

/// <summary>
/// A long-running hosted service that periodically triggers the ingestion cycle.
/// Interval is configurable via "Ingestion:IntervalMinutes" in appsettings.json.
/// </summary>
public class IngestionBackgroundService : BackgroundService // ← Implements IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIngestionCycleCoordinator _coordinator;
    private readonly ILogger<IngestionBackgroundService> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Brief delay before the first run so the host can finish starting.
    /// </summary>
    private static readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="IngestionBackgroundService"/> class.
    /// </summary>
    /// <param name="scopeFactory">Factory used to create a scope per ingestion cycle so scoped services (e.g. MediatR) resolve correctly.</param>
    /// <param name="logger">Logger for ingestion diagnostics.</param>
    /// <param name="configuration">Application configuration; reads <c>Ingestion:IntervalMinutes</c> for the polling interval.</param>
    public IngestionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IIngestionCycleCoordinator coordinator,
        ILogger<IngestionBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _coordinator = coordinator;
        _logger = logger;

        // Can't inject MediatR in class constructor because they have different lifecycle

        // Read interval from appsettings.json
        var minutes = configuration.GetValue<int>("Ingestion:IntervalMinutes", 60);
        _interval = TimeSpan.FromMinutes(minutes);
    }

    /// <summary>
    /// Runs the ingestion loop until the host stops: executes <see cref="RunIngestionCycleCommand"/> via MediatR and waits for the configured interval between runs.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token signaled when the host is shutting down.</param>
    /// <returns>A task that completes when cancellation is requested.</returns>
    /// <remarks>
    /// Uses a short initial delay so the application can finish starting before the first cycle.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Ingestion Background Service started. Interval: {Interval}. Initial delay: {Delay}.",
            _interval, _initialDelay);

        // Wait for the application to fully initialize before the first run.
        await DelayInitialAsync(stoppingToken);
        _coordinator.SetNextRunAtUtc(DateTime.UtcNow);

        try
        {
            while (!stoppingToken.IsCancellationRequested) // Infinite loop until app stops
            {

                // This wraps the entire ingestion cycle. It creates a safe boundary and trigger the main
                // Command Handler without polluting the Singleton host.
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var runResult = await mediator.Send(new RunIngestionCycleCommand(), stoppingToken);
                if (runResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Ingestion cycle failed: {ErrorCode} - {ErrorMessage}",
                        runResult.Error.Code,
                        runResult.Error.Message);
                }

                // Wait for the configured interval before the next run.
                if (!stoppingToken.IsCancellationRequested)
                {
                    _coordinator.SetNextRunAtUtc(DateTime.UtcNow.Add(_interval));
                    _logger.LogInformation("Next ingestion cycle in {Interval}.", _interval);
                    await DelayBetweenCyclesAsync(stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Ingestion Background Service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ingestion cycle. Will retry after interval.");
        }
    }

    /// <summary>Delay before the first ingestion cycle (override in tests to avoid real time).</summary>
    protected virtual Task DelayInitialAsync(CancellationToken stoppingToken)
        => Task.Delay(_initialDelay, stoppingToken);

    /// <summary>Delay between completed cycles (override in tests to avoid real time).</summary>
    protected virtual Task DelayBetweenCyclesAsync(CancellationToken stoppingToken)
        => Task.Delay(_interval, stoppingToken);
}