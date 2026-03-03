using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.BackgroundJobs;

/// <summary>
/// A long-running hosted service that periodically triggers the ingestion cycle.
/// Interval is configurable via "Ingestion:IntervalMinutes" in appsettings.json.
/// </summary>
public class IngestionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IngestionBackgroundService> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Initial delay before the first run so the application has time to fully start.
    /// </summary>
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(15);

    public IngestionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<IngestionBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = configuration.GetValue<int>("Ingestion:IntervalMinutes", 60);
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Ingestion Background Service started. Interval: {Interval}. Initial delay: {Delay}.",
            _interval, InitialDelay);

        // Wait for the application to fully initialize before the first run.
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ingestionService = scope.ServiceProvider.GetRequiredService<IIngestionService>();
                await ingestionService.RunIngestionCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Ingestion Background Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in ingestion cycle. Will retry after interval.");
            }

            // Wait for the configured interval before the next run.
            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Next ingestion cycle in {Interval}.", _interval);
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}