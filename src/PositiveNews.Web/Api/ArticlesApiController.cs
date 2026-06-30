using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PositiveNews.Application.Options;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API for browsing article feeds and retrieving article details.
/// </summary>
/// <param name="mediator">MediatR pipeline for article queries.</param>
/// <param name="feedOptions">Article feed paging configuration.</param>
[ApiController]
[Route("api/articles")]
public sealed class ArticlesApiController(IMediator mediator, IOptions<ArticleFeedOptions> feedOptions) : ControllerBase
{

    /// <summary>
    /// Returns a paginated feed of articles with optional topic filters and sort order.
    /// </summary>
    /// <param name="request">Feed query string payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Paged article previews and metadata, or RFC 7807 errors for invalid/non-existent requests.</returns>
    [HttpGet("feed")]
    [ProducesResponseType(typeof(ArticleFeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleFeedResponse>> GetFeed(
        [FromQuery] GetArticleFeedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(request.ToGetArticleFeedQuery(feedOptions.Value), cancellationToken);
        return result
            .Map(feed => feed.ToArticleFeedResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Returns full detail for a single article by identifier.
    /// </summary>
    /// <param name="id">Article primary key.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Article detail, or a problem response when the article cannot be loaded.</returns>
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
