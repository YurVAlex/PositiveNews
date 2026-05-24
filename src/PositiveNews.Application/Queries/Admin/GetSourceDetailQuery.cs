using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

/// <summary>
/// Returns detailed source metadata for the admin editor.
/// </summary>
public sealed record GetSourceDetailQuery(int SourceId) : IRequest<Result<SourceAdminDetailDto>>;
