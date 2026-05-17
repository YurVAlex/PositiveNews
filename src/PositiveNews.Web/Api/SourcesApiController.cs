using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API exposing news sources for article filtering UI.
/// </summary>
/// <param name="mediator">MediatR pipeline for source list queries.</param>
[ApiController]
[Route("api/sources")]
public sealed class SourcesApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns all active sources available for filtering the article feed.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Metadata containing the ordered list of sources.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SourcesMetadataResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SourcesMetadataResponse>> GetSources(CancellationToken cancellationToken = default)
    {
        var items = await mediator.Send(new GetSourceFilterListQuery(), cancellationToken);
        return Ok(new SourcesMetadataResponse { Sources = items.ToSourceFilterItemResponses() });
    }
}
