using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API exposing topic names for article filtering UI.
/// </summary>
/// <param name="mediator">MediatR pipeline for topic list queries.</param>
[ApiController]
[Route("api/topics")]
public sealed class TopicsApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns all topic names available for filtering the article feed.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Metadata containing the ordered list of topic names.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TopicsMetadataResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TopicsMetadataResponse>> GetTopics(CancellationToken cancellationToken = default)
    {
        var names = await mediator.Send(new GetTopicFilterListQuery(), cancellationToken);
        return Ok(new TopicsMetadataResponse { TopicNames = names });
    }
}
