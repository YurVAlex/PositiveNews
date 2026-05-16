using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Application.QueryHandlers.Articles;
using System.Linq;

namespace PositiveNews.Application.Tests.Articles;

public class GetArticleFeedQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_RequestedTopicDoesNotExist()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var topicReadRepository = Substitute.For<ITopicReadRepository>();
        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        topicReadRepository
            .GetTopicNamesAsync(Arg.Any<CancellationToken>())
            .Returns(["Space", "Health"]);

        var sut = new GetArticleFeedQueryHandler(articleReadRepository, topicReadRepository, sourceReadRepository);

        var result = await sut.Handle(new GetArticleFeedQuery(1, ["Space", "Unknown"]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ArticleFeed.TopicNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await articleReadRepository
            .DidNotReceive()
            .GetFeedPageAsync(Arg.Any<ArticleFeedFilter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_RequestedSourceDoesNotExist()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var topicReadRepository = Substitute.For<ITopicReadRepository>();
        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        sourceReadRepository
            .GetExistingSourceIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([1]);

        var sut = new GetArticleFeedQueryHandler(articleReadRepository, topicReadRepository, sourceReadRepository);

        var result = await sut.Handle(new GetArticleFeedQuery(1, SourceIds: [1, 99]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ArticleFeed.SourceNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await articleReadRepository
            .DidNotReceive()
            .GetFeedPageAsync(Arg.Any<ArticleFeedFilter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_RequestedPageDoesNotExist()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        articleReadRepository
            .GetFeedPageAsync(Arg.Any<ArticleFeedFilter>(), Arg.Any<CancellationToken>())
            .Returns(new ArticleFeedPageResult
            {
                CurrentPage = 3,
                TotalPages = 2,
                PageSize = 10
            });

        var topicReadRepository = Substitute.For<ITopicReadRepository>();
        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        topicReadRepository
            .GetTopicNamesAsync(Arg.Any<CancellationToken>())
            .Returns(["Space", "Health"]);

        var sut = new GetArticleFeedQueryHandler(articleReadRepository, topicReadRepository, sourceReadRepository);

        var result = await sut.Handle(new GetArticleFeedQuery(3, ["Space"]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ArticleFeed.PageNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFeed_When_RequestIsValid()
    {
        var expectedPage = new ArticleFeedPageResult
        {
            CurrentPage = 1,
            TotalPages = 2,
            PageSize = 10,
            SelectedTopics = ["Space"]
        };

        var expectedSequence = new[] { "Space" };
        var expectedSourceIds = new[] { 2, 3 };

        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        articleReadRepository
            .GetFeedPageAsync(Arg.Any<ArticleFeedFilter>(), Arg.Any<CancellationToken>())
            .Returns(expectedPage);

        var topicReadRepository = Substitute.For<ITopicReadRepository>();
        topicReadRepository
            .GetTopicNamesAsync(Arg.Any<CancellationToken>())
            .Returns(["Space", "Health"]);

        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        sourceReadRepository
            .GetExistingSourceIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([2, 3]);

        var sut = new GetArticleFeedQueryHandler(articleReadRepository, topicReadRepository, sourceReadRepository);

        var result = await sut.Handle(
            new GetArticleFeedQuery(1, ["Space", "space"], SourceIds: [2, 2, 3]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedPage);
        await articleReadRepository.Received(1).GetFeedPageAsync(
            Arg.Is<ArticleFeedFilter>(f =>
                f.Page == 1 &&
                f.PageSize == 10 &&
                f.SortBy == ArticleFeedSortBy.PublishedAt &&
                f.Topics.SequenceEqual(expectedSequence) &&
                f.SourceIds.SequenceEqual(expectedSourceIds)),
            Arg.Any<CancellationToken>());
    }
}
