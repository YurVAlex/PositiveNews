using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;
using PositiveNews.Application.QueryHandlers.Admin;

namespace PositiveNews.Application.Tests.Admin;

public class GetAdminCommentDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnDetail_When_CommentFound()
    {
        var detail = new CommentAdminDetailDto
        {
            Id = 5,
            Content = "Test comment",
            UserId = 2,
            UserName = "Jane",
            IsActive = true,
            ArticleId = 1,
        };

        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        commentReadRepository.GetAdminDetailByIdAsync(5, Arg.Any<CancellationToken>()).Returns(detail);
        var handler = new GetAdminCommentDetailQueryHandler(commentReadRepository);

        var result = await handler.Handle(new GetAdminCommentDetailQuery(5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        result.Value.Content.Should().Be("Test comment");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CommentMissing()
    {
        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        commentReadRepository.GetAdminDetailByIdAsync(99, Arg.Any<CancellationToken>()).Returns((CommentAdminDetailDto?)null);
        var handler = new GetAdminCommentDetailQueryHandler(commentReadRepository);

        var result = await handler.Handle(new GetAdminCommentDetailQuery(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Admin.CommentNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
