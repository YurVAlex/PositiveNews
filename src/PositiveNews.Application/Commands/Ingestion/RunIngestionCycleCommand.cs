using MediatR;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed record RunIngestionCycleCommand : IRequest;
