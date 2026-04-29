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
        logger.LogInformation(
            "Ingestion settings refreshed: {CommonWordCount} positive words, {SourceCount} source rules.",
            snapshot.Common.PositiveWords.Count,
            snapshot.Sources.Count);
        return Task.FromResult(snapshot);
    }
}
