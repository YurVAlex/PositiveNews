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
}
