using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Runs a full ingestion pass over all active RSS sources in sequence.
/// </summary>
public sealed record RunIngestionCycleCommand : IRequest<Result>;
