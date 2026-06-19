using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Queries.Comments;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API for article comments and complaints.
/// </summary>
[ApiController]
[Route("api/articles/{articleId:long}/comments")]
public sealed class ArticleCommentsApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns active top-level comments for an article.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ArticleCommentsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleCommentsListResponse>> GetComments(
        long articleId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetArticleCommentsQuery(articleId), cancellationToken);
        return result
            .Map(comments => comments.ToArticleCommentsListResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Creates a new top-level comment on an article.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponse>> AddComment(
        long articleId,
        [FromBody] AddArticleCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return UnauthorizedProblem();
        }

        var result = await mediator.Send(request.ToAddArticleCommentCommand(articleId, userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.Map(dto => dto.ToCommentResponse()).ToActionResult(this);
        }

        return CreatedAtAction(
            nameof(GetComments),
            new { articleId },
            result.Value.ToCommentResponse());
    }

    /// <summary>
    /// Files a complaint against a comment.
    /// </summary>
    [HttpPost("{commentId:long}/complains")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitComplaint(
        long articleId,
        long commentId,
        [FromBody] SubmitCommentComplaintRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return UnauthorizedProblem();
        }

        var result = await mediator.Send(
            request.ToSubmitCommentComplaintCommand(articleId, commentId, userId),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(userIdValue, out userId);
    }

    private ObjectResult UnauthorizedProblem()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Invalid or missing user identifier in the security context.",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };
        ProblemDetailsTraceExtensions.EnrichWithTrace(HttpContext, problemDetails);
        return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }
}
