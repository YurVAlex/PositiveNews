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
public class IngestionBackgroundService : BackgroundService // ← Implements IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IngestionBackgroundService> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Initial delay before the first run so the application has time to fully start.
    /// </summary>
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(15);

    public IngestionBackgroundService(   // Host Creates instances of IngestionBackgroundService when starts
        IServiceScopeFactory scopeFactory, // This service added to ASP .NET IoC by default. Needed for creation scope in singleton
        ILogger<IngestionBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // Read interval from appsettings.json
        var minutes = configuration.GetValue<int>("Ingestion:IntervalMinutes", 60);
        _interval = TimeSpan.FromMinutes(minutes);
    }

    // THIS METHOD IS CALLED AUTOMATICALLY BY ASP.NET CORE WHEN HOST STARTS
    // ASP.NET Core host calls ExecuteAsync(CancellationToken) for each service after app.RunAsync() in programm.cs
    // because of BackgroundService implement IHostedService
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Ingestion Background Service started. Interval: {Interval}. Initial delay: {Delay}.",
            _interval, InitialDelay);

        // Wait for the application to fully initialize before the first run.
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested) // Infinite loop until app stops
        {
            try
            {
                using var scope = _scopeFactory.CreateScope(); // Create a fresh scope for this cycle
                var ingestionService = scope.ServiceProvider.GetRequiredService<IIngestionService>();

                await ingestionService.RunIngestionCycleAsync(stoppingToken);       // ---=== EXECUTE INGESTION ===---
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Ingestion Background Service is stopping.");
                break; // ← Exit loop on shutdown
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