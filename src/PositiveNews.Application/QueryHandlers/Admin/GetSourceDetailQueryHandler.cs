using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

/// <summary>
/// Loads admin edit details for a single source.
/// </summary>
public sealed class GetSourceDetailQueryHandler(ISourceReadRepository sourceReadRepository)
    : IRequestHandler<GetSourceDetailQuery, Result<SourceAdminDetailDto>>
{
    public async Task<Result<SourceAdminDetailDto>> Handle(
        GetSourceDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (request.SourceId <= 0)
        {
            return Result<SourceAdminDetailDto>.Failure(
                new Error("Admin.SourceIdInvalid", "Source id must be a positive integer.", ErrorType.Validation));
        }

        var detail = await sourceReadRepository.GetAdminSourceDetailAsync(request.SourceId, cancellationToken);
        if (detail is null)
        {
            return Result<SourceAdminDetailDto>.Failure(
                new Error("Admin.SourceNotFound", $"Source with id '{request.SourceId}' was not found.", ErrorType.NotFound));
        }

        return Result<SourceAdminDetailDto>.Success(detail);
    }
}
