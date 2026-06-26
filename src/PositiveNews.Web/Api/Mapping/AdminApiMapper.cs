using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Maps admin DTOs to wire-format API responses.
/// </summary>
[Mapper]
public static partial class AdminApiMapper
{
    public static partial ArticleAdminItemResponse ToArticleAdminItemResponse(this ArticleAdminItemDto source);
    public static partial IReadOnlyList<ArticleAdminItemResponse> ToArticleAdminItemResponses(this IReadOnlyList<ArticleAdminItemDto> source);
    public static partial ArticleAdminDetailResponse ToArticleAdminDetailResponse(this ArticleAdminDetailDto source);
    public static partial UserAdminItemResponse ToUserAdminItemResponse(this UserAdminItemDto source);
    public static partial IReadOnlyList<UserAdminItemResponse> ToUserAdminItemResponses(this IReadOnlyList<UserAdminItemDto> source);
    public static partial UserAdminDetailResponse ToUserAdminDetailResponse(this UserAdminDetailDto source);
    public static partial CommentAdminDetailResponse ToCommentAdminDetailResponse(this CommentAdminDetailDto source);
    public static partial CommentComplaintAdminItemResponse ToCommentComplaintAdminItemResponse(this CommentComplaintAdminItemDto source);
    public static partial AuditLogAdminItemResponse ToAuditLogAdminItemResponse(this PositiveNews.Application.DTOs.Admin.AuditLogAdminItemDto source);
    public static partial IReadOnlyList<AuditLogAdminItemResponse> ToAuditLogAdminItemResponses(this IReadOnlyList<PositiveNews.Application.DTOs.Admin.AuditLogAdminItemDto> source);
}
