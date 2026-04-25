using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesApiController(IMediator mediator) : ControllerBase
{
    [HttpGet("feed")]
    [ProducesResponseType(typeof(ArticleFeedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ArticleFeedResponse>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] string? topic = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetArticleFeedQuery(page, topic), cancellationToken);
        return Ok(result.ToArticleFeedResponse());
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ArticleDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleDetailResponse>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var article = await mediator.Send(new GetArticleDetailQuery(id), cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        return Ok(article.ToArticleDetailResponse());
    }
}
