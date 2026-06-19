using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.CommandHandlers.Comments;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Comments;

public class AddArticleCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_PersistComment_When_ArticleAndUserExist()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var commentWriteRepository = Substitute.For<ICommentWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        articleReadRepository.ExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        userReadRepository.FindByIdWithRolesAsync(2, Arg.Any<CancellationToken>())
            .Returns(User.Create("user@example.com", "Alice"));

        var sut = new AddArticleCommentCommandHandler(
            articleReadRepository,
            userReadRepository,
            commentWriteRepository,
            unitOfWork);

        var result = await sut.Handle(new AddArticleCommentCommand(1, 2, "Hello"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserName.Should().Be("Alice");
        result.Value.Content.Should().Be("Hello");
        commentWriteRepository.Received(1).Add(Arg.Any<Comment>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleMissing()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var commentWriteRepository = Substitute.For<ICommentWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        articleReadRepository.ExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new AddArticleCommentCommandHandler(
            articleReadRepository,
            userReadRepository,
            commentWriteRepository,
            unitOfWork);

        var result = await sut.Handle(new AddArticleCommentCommand(1, 2, "Hello"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Article.NotFound");
        commentWriteRepository.DidNotReceive().Add(Arg.Any<Comment>());
    }
}
