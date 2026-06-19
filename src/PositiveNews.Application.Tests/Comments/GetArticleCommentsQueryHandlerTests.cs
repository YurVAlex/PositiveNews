using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Application.Queries.Comments;
using PositiveNews.Application.QueryHandlers.Comments;

namespace PositiveNews.Application.Tests.Comments;

public class GetArticleCommentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnComments_When_ArticleExists()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        var comments = new List<CommentListItemDto>
        {
            new() { Id = 1, UserId = 2, UserName = "Alice", Content = "Nice", CreatedAt = DateTime.UtcNow }
        };

        articleReadRepository.ExistsActiveAsync(5, Arg.Any<CancellationToken>()).Returns(true);
        commentReadRepository.GetActiveTopLevelByArticleIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(comments);

        var sut = new GetArticleCommentsQueryHandler(articleReadRepository, commentReadRepository);

        var result = await sut.Handle(new GetArticleCommentsQuery(5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].UserName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleMissing()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var commentReadRepository = Substitute.For<ICommentReadRepository>();

        articleReadRepository.ExistsActiveAsync(99, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new GetArticleCommentsQueryHandler(articleReadRepository, commentReadRepository);

        var result = await sut.Handle(new GetArticleCommentsQuery(99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Article.NotFound");
        await commentReadRepository.DidNotReceive()
            .GetActiveTopLevelByArticleIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}
