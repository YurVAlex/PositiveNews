using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed record ProcessIngestionSourceCommand(
    IngestionSourceSnapshot Source,
    TopicLookup TopicLookup,
    IngestionSettingsSnapshot IngestionSettings) : IRequest<Result<int>>;
