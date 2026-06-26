using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Mapperly mappings between source filter DTOs and HTTP API response models.
/// </summary>
[Mapper]
public static partial class SourceApiMapper
{
    /// <summary>
    /// Maps an application source filter row to the wire-format response item.
    /// </summary>
    public static partial SourceFilterItemResponse ToSourceFilterItemResponse(this SourceFilterItemDto source);

    /// <summary>
    /// Maps a sequence of source filter rows to response items.
    /// </summary>
    public static partial IReadOnlyList<SourceFilterItemResponse> ToSourceFilterItemResponses(
        this IReadOnlyList<SourceFilterItemDto> source);

    /// <summary>
    /// Maps an admin source list item DTO to the wire-format response model.
    /// </summary>
    public static partial SourceAdminItemResponse ToSourceAdminItemResponse(this SourceAdminItemDto source);

    /// <summary>
    /// Maps a sequence of admin source list item DTOs to response models.
    /// </summary>
    public static partial IReadOnlyList<SourceAdminItemResponse> ToSourceAdminItemResponses(
        this IReadOnlyList<SourceAdminItemDto> source);

    /// <summary>
    /// Maps an admin source detail DTO to the wire-format edit response model.
    /// </summary>
    public static partial SourceAdminDetailResponse ToSourceAdminDetailResponse(this SourceAdminDetailDto source);
}
