using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class RefreshIngestionSettingsCommandHandler(
    IIngestionSettingsProvider settingsProvider,
    ILogger<RefreshIngestionSettingsCommandHandler> logger)
    : IRequestHandler<RefreshIngestionSettingsCommand, IngestionSettingsSnapshot>
{
    public Task<IngestionSettingsSnapshot> Handle(
        RefreshIngestionSettingsCommand request, CancellationToken cancellationToken)
    {
        var snapshot = settingsProvider.GetCurrentSettings();

        logger.LogInformation("Ingestion settings refreshed.");

        return Task.FromResult(snapshot);
    }
}
