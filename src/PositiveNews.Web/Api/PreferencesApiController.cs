using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PositiveNews.Application.Commands.FeedPreferences;
using PositiveNews.Application.Queries.FeedPreferences;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API for persisted user feed preferences.
/// </summary>
[ApiController]
[Route("api/users/me")]
[Authorize]
public sealed class PreferencesApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the current user's saved feed preferences.
    /// </summary>
    [HttpGet("feed-preferences")]
    [ProducesResponseType(typeof(UserFeedPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserFeedPreferencesResponse>> GetFeedPreferences(CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return UnauthorizedProblem();
        }

        var result = await mediator.Send(new GetUserFeedPreferencesQuery(userId), cancellationToken);
        return result
            .Map(dto => dto.ToUserFeedPreferencesResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Replaces the current user's feed preference snapshot.
    /// </summary>
    [HttpPut("feed-preferences")]
    [ProducesResponseType(typeof(UserFeedPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserFeedPreferencesResponse>> PutFeedPreferences(
        [FromBody] UpdateUserFeedPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return UnauthorizedProblem();
        }

        var result = await mediator.Send(request.ToUpdateUserFeedPreferencesCommand(userId), cancellationToken);
        return result
            .Map(dto => dto.ToUserFeedPreferencesResponse())
            .ToActionResult(this);
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
