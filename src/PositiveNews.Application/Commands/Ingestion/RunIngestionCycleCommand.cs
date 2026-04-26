using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed record RunIngestionCycleCommand : IRequest<Result>;
