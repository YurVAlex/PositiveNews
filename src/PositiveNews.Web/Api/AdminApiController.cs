using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

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
}
