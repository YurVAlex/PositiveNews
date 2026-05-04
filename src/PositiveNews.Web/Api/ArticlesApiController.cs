using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesApiController(IMediator mediator) : ControllerBase
{
    private static ArticleFeedSortBy MapSortQuery(string? sort)
    {
        return string.Equals(sort, "positivity", StringComparison.OrdinalIgnoreCase)
            ? ArticleFeedSortBy.PositivityScore
            : ArticleFeedSortBy.PublishedAt;
    }

    [HttpGet("feed")]
    [ProducesResponseType(typeof(ArticleFeedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ArticleFeedResponse>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] string[]? topic = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> topics = topic ?? Array.Empty<string>();
        var sortBy = MapSortQuery(sort);
        var result = await mediator.Send(new GetArticleFeedQuery(page, topics, SortBy: sortBy), cancellationToken);
        return Ok(result.ToArticleFeedResponse());
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ArticleDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDetailResponse>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetArticleDetailQuery(id), cancellationToken);
        return result
            .Map(article => article.ToArticleDetailResponse())
            .ToActionResult(this);
    }
}
