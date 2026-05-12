using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Fetches and processes one RSS source using shared topic lookup and ingestion settings.
/// </summary>
/// <param name="Source">Snapshot of the source row including feed URL.</param>
/// <param name="TopicLookup">Pre-built lookup for topic normalization.</param>
/// <param name="IngestionSettings">Cleaner, validation, and positivity configuration.</param>
public sealed record ProcessIngestionSourceCommand(
    IngestionSourceSnapshot Source,
    TopicLookup TopicLookup,
    IngestionSettingsSnapshot IngestionSettings) : IRequest<Result<int>>;
