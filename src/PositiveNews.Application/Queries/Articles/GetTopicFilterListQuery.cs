using MediatR;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>Returns topic names for the feed filter UI (ordered for display).</summary>
public sealed record GetTopicFilterListQuery : IRequest<IReadOnlyList<string>>;
