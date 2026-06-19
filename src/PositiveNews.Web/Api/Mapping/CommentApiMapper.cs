using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Mapperly mappings for article comment API models.
/// </summary>
[Mapper]
public static partial class CommentApiMapper
{
    /// <summary>
    /// Maps a comment DTO to the wire response.
    /// </summary>
    public static partial CommentResponse ToCommentResponse(this CommentListItemDto source);

    /// <summary>
    /// Maps a created comment DTO to the wire response.
    /// </summary>
    public static partial CommentResponse ToCommentResponse(this CommentCreatedDto source);

    /// <summary>
    /// Maps a list of comment DTOs to the wire list response.
    /// </summary>
    public static ArticleCommentsListResponse ToArticleCommentsListResponse(
        this IReadOnlyList<CommentListItemDto> source)
    {
        return new ArticleCommentsListResponse
        {
            Comments = source.Select(c => c.ToCommentResponse()).ToList()
        };
    }

    /// <summary>
    /// Maps an add-comment request to the application command.
    /// </summary>
    public static AddArticleCommentCommand ToAddArticleCommentCommand(
        this AddArticleCommentRequest source,
        long articleId,
        long userId)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new AddArticleCommentCommand(articleId, userId, source.Content);
    }

    /// <summary>
    /// Maps a submit-complaint request to the application command.
    /// </summary>
    public static SubmitCommentComplaintCommand ToSubmitCommentComplaintCommand(
        this SubmitCommentComplaintRequest source,
        long articleId,
        long commentId,
        long userId)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new SubmitCommentComplaintCommand(articleId, commentId, userId, source.Reason);
    }
}
