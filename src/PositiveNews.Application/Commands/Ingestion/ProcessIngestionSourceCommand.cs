using MediatR;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed record ProcessIngestionSourceCommand(
    IngestionSourceSnapshot Source,
    TopicLookup TopicLookup) : IRequest;
