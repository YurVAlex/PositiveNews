using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

[ApiController]
[Route("api/topics")]
public sealed class TopicsApiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TopicsMetadataResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TopicsMetadataResponse>> GetTopics(CancellationToken cancellationToken = default)
    {
        var names = await mediator.Send(new GetTopicFilterListQuery(), cancellationToken);
        return Ok(new TopicsMetadataResponse { TopicNames = names });
    }
}
