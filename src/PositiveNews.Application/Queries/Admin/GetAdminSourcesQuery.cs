using MediatR;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

/// <summary>
/// Returns all source rows for admin management.
/// </summary>
public sealed record GetAdminSourcesQuery : IRequest<IReadOnlyList<SourceAdminItemDto>>;
