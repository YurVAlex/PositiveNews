using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Queries.Admin;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// Administrative endpoints restricted to users in the Admin role.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Simple health-style endpoint confirming administrative access.
    /// </summary>
    /// <returns>JSON indicating that admin authorization succeeded.</returns>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new { ok = true, message = "Admin access granted." });
    }

    /// <summary>
    /// Returns whether an ingestion cycle is running and when the next scheduled run is expected.
    /// </summary>
    [HttpGet("ingestion/status")]
    [ProducesResponseType(typeof(IngestionCycleStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestionCycleStatusDto>> GetIngestionStatus(CancellationToken cancellationToken)
    {
        var status = await mediator.Send(new GetIngestionCycleStatusQuery(), cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Returns the latest ingestion runs for the admin table.
    /// </summary>
    [HttpGet("ingestion/runs")]
    [ProducesResponseType(typeof(IReadOnlyList<IngestionRunListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IngestionRunListItemDto>>> GetIngestionRuns(
        CancellationToken cancellationToken)
    {
        var runs = await mediator.Send(new GetIngestionRunsQuery(), cancellationToken);
        return Ok(runs);
    }

    /// <summary>
    /// Returns the admin view of all sources.
    /// </summary>
    [HttpGet("sources")]
    [ProducesResponseType(typeof(IReadOnlyList<SourceAdminItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SourceAdminItemResponse>>> GetSources(CancellationToken cancellationToken)
    {
        var sources = await mediator.Send(new GetAdminSourcesQuery(), cancellationToken);
        return Ok(sources.ToSourceAdminItemResponses());
    }

    /// <summary>
    /// Returns the editable admin view for a specific source.
    /// </summary>
    [HttpGet("sources/{sourceId:int}")]
    [ProducesResponseType(typeof(SourceAdminDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SourceAdminDetailResponse>> GetSourceDetail(int sourceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSourceDetailQuery(sourceId), cancellationToken);
        return result
            .Map(dto => dto.ToSourceAdminDetailResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Updates an existing source with admin moderation decisions.
    /// </summary>
    [HttpPut("sources/{sourceId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSource(int sourceId, [FromBody] UpdateSourceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var moderatorId))
        {
            return UnauthorizedProblem();
        }

        var command = new UpdateSourceCommand(sourceId,
            request.TrustScore,
            request.IsActive,
            request.FeedUrl,
            request.Reason,
            request.Note,
            moderatorId);

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }

    /// <summary>
    /// Starts an ingestion cycle in the background when one is not already running.
    /// </summary>
    [HttpPost("ingestion/trigger")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TriggerIngestionCycle(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new TriggerIngestionCycleCommand(), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return Accepted();
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
