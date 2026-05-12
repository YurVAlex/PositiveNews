using MediatR;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API for browsing article feeds and retrieving article details.
/// </summary>
/// <param name="mediator">MediatR pipeline for article queries.</param>
[ApiController]
[Route("api/articles")]
public sealed class ArticlesApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Maps the optional sort query string to a feed sort strategy.
    /// </summary>
    /// <param name="sort">Raw sort query value (e.g. <c>positivity</c>).</param>
    /// <returns>Sort by positivity score when requested; otherwise by publication time.</returns>
    private static ArticleFeedSortBy MapSortQuery(string? sort)
    {
        return string.Equals(sort, "positivity", StringComparison.OrdinalIgnoreCase)
            ? ArticleFeedSortBy.PositivityScore
            : ArticleFeedSortBy.PublishedAt;
    }

    /// <summary>
    /// Returns a paginated feed of articles with optional topic filters and sort order.
    /// </summary>
    /// <param name="page">1-based page index.</param>
    /// <param name="topic">Optional topic filters (repeatable query parameter).</param>
    /// <param name="sort">Optional sort mode (e.g. positivity).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Paged article previews and metadata.</returns>
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
