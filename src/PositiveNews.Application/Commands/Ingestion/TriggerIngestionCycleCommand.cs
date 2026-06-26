using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>Requests an immediate ingestion cycle from the admin panel.</summary>
public sealed record TriggerIngestionCycleCommand : IRequest<Result>;
