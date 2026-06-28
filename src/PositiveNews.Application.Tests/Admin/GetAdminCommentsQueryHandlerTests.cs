using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;
using PositiveNews.Application.QueryHandlers.Admin;

namespace PositiveNews.Application.Tests.Admin;

public class GetAdminCommentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnActiveComments_When_RepositoryReturnsList()
    {
        IReadOnlyList<CommentAdminItemDto> items =
        [
            new CommentAdminItemDto { Id = 1, ArticleId = 10, UserId = 2, ComplaintCount = 3, IsActive = true },
        ];

        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        commentReadRepository.GetAdminActiveCommentsAsync(Arg.Any<CancellationToken>()).Returns(items);
        var handler = new GetAdminCommentsQueryHandler(commentReadRepository);

        var result = await handler.Handle(new GetAdminCommentsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].ComplaintCount.Should().Be(3);
    }
}
