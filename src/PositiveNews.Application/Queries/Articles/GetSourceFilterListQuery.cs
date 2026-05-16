using MediatR;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>Returns active sources for the feed filter UI (ordered for display).</summary>
public sealed record GetSourceFilterListQuery : IRequest<IReadOnlyList<SourceFilterItemDto>>;
