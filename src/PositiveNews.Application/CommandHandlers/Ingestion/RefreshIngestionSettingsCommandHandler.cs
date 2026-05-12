using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

/// <summary>
/// Loads the latest ingestion configuration snapshot for downstream handlers.
/// </summary>
/// <param name="settingsProvider">Provides merged ingestion settings from configuration or database.</param>
/// <param name="logger">Logs refresh events.</param>
public sealed class RefreshIngestionSettingsCommandHandler(
    IIngestionSettingsProvider settingsProvider,
    ILogger<RefreshIngestionSettingsCommandHandler> logger)
    : IRequestHandler<RefreshIngestionSettingsCommand, IngestionSettingsSnapshot>
{
    /// <summary>
    /// Retrieves current cleaner, validation, positivity, and per-source rules from the provider.
    /// </summary>
    /// <param name="request">Marker command with no payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Immutable settings snapshot.</returns>
    public Task<IngestionSettingsSnapshot> Handle(
        RefreshIngestionSettingsCommand request, CancellationToken cancellationToken)
    {
        var snapshot = settingsProvider.GetCurrentSettings();

        logger.LogInformation("Ingestion settings refreshed.");

        return Task.FromResult(snapshot);
    }
}
