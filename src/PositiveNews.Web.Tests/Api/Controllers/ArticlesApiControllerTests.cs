using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;
using System.Linq;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class ArticlesApiControllerTests
{
    private static readonly string[] TopicsSpaceHealth = ["Space", "Health"];
    private static readonly string[] TopicsTech = ["Tech"];
    private static readonly string[] BrokenTopics = ["Spaces", "Health", "YurVAlex"];

    [Fact]
    public async Task GetFeed_Should_ReturnArticleFeedResponse_When_HandlerSucceeds()
    {
        var page = TestDataBuilders.ArticleFeedPage();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleFeedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleFeedPageResult>.Success(page)));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        using var cts = new CancellationTokenSource();
        var result = await sut.GetFeed(
            new GetArticleFeedRequest { Page = 1, Topic = TopicsSpaceHealth },
            cts.Token);

        await mediator.Received(1).Send(
            Arg.Is<GetArticleFeedQuery>(q =>
                q.Page == 1 &&
                q.Topics != null &&
                q.Topics.SequenceEqual(TopicsSpaceHealth) &&
                q.SortBy == ArticleFeedSortBy.PublishedAt),
            cts.Token);
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFeed_Should_ReturnValidationProblem_When_HandlerReturnsValidationFailure()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleFeedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleFeedPageResult>.Failure(
                new Error("Validation.Failed", "Validation failed.", ErrorType.Validation))));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        using var cts = new CancellationTokenSource();
        var result = await sut.GetFeed(
            new GetArticleFeedRequest { Page = -1, Topic = BrokenTopics, Sort = "Trust" },
            cts.Token);

        await mediator.Received(1).Send(
            Arg.Is<GetArticleFeedQuery>(q =>
                q.Page == -1 &&
                q.Topics != null &&
                q.Topics.SequenceEqual(BrokenTopics) &&
                !Enum.IsDefined(typeof(ArticleFeedSortBy), q.SortBy)),
            cts.Token);
        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetFeed_Should_UsePreferencesSort_When_SortQueryIsPreferences()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleFeedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleFeedPageResult>.Success(TestDataBuilders.ArticleFeedPage())));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        await sut.GetFeed(
            new GetArticleFeedRequest { Page = 1, Topic = TopicsTech, Source = [2], Sort = "preferences" },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetArticleFeedQuery>(q =>
                q.SortBy == ArticleFeedSortBy.Preferences &&
                q.Topics != null &&
                q.Topics.SequenceEqual(TopicsTech)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeed_Should_UsePositivitySort_When_SortQueryIsPositivity()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleFeedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleFeedPageResult>.Success(TestDataBuilders.ArticleFeedPage())));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        await sut.GetFeed(
            new GetArticleFeedRequest { Page = 2, Topic = TopicsTech, Sort = "POSITIVITY" },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetArticleFeedQuery>(q =>
                q.Page == 2 &&
                q.SortBy == ArticleFeedSortBy.PositivityScore &&
                q.Topics != null &&
                q.Topics.SequenceEqual(TopicsTech)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeed_Should_ReturnNotFoundProblem_When_HandlerReturnsNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleFeedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleFeedPageResult>.Failure(
                new Error("ArticleFeed.PageNotFound", "missing", ErrorType.NotFound))));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.GetFeed(new GetArticleFeedRequest { Page = 5 }, CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetById_Should_ReturnOk_When_ArticleExists()
    {
        var detail = TestDataBuilders.ArticleDetail();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleDetailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleDetailDto>.Success(detail)));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.GetById(42, CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetArticleDetailQuery>(q => q.Id == 42),
            Arg.Any<CancellationToken>());
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_Should_ReturnNotFoundProblem_When_HandlerReturnsNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleDetailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ArticleDetailDto>.Failure(
                new Error("Article.Missing", "gone", ErrorType.NotFound))));

        var sut = new ArticlesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.GetById(99, CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}