using MediatR;
using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Requests a fresh snapshot of ingestion configuration from the settings provider.
/// </summary>
public sealed record RefreshIngestionSettingsCommand : IRequest<IngestionSettingsSnapshot>;
