using FluentAssertions;
using PositiveNews.Application.Common;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.CommandHandlers.Comments;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Comments;

public class SubmitCommentComplaintCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_PersistComplaint_When_Valid()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        var complaintWriteRepository = Substitute.For<IComplaintWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        articleReadRepository.ExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        commentReadRepository.GetActiveByIdForArticleAsync(5, 1, Arg.Any<CancellationToken>())
            .Returns(new ActiveCommentDto { Id = 5, UserId = 10, ArticleId = 1 });
        complaintWriteRepository.ExistsForUserAndCommentAsync(2, 5, Arg.Any<CancellationToken>())
            .Returns(false);

        var sut = new SubmitCommentComplaintCommandHandler(
            articleReadRepository,
            commentReadRepository,
            complaintWriteRepository,
            unitOfWork);

        var result = await sut.Handle(
            new SubmitCommentComplaintCommand(1, 5, 2, "Inappropriate"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        complaintWriteRepository.Received(1).Add(Arg.Any<Complaint>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_SelfComplaint()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        var complaintWriteRepository = Substitute.For<IComplaintWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        articleReadRepository.ExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        commentReadRepository.GetActiveByIdForArticleAsync(5, 1, Arg.Any<CancellationToken>())
            .Returns(new ActiveCommentDto { Id = 5, UserId = 2, ArticleId = 1 });

        var sut = new SubmitCommentComplaintCommandHandler(
            articleReadRepository,
            commentReadRepository,
            complaintWriteRepository,
            unitOfWork);

        var result = await sut.Handle(
            new SubmitCommentComplaintCommand(1, 5, 2, "Inappropriate"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ErrorCodes.Comment.SelfComplaint);
        complaintWriteRepository.DidNotReceive().Add(Arg.Any<Complaint>());
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_AlreadySubmitted()
    {
        var articleReadRepository = Substitute.For<IArticleReadRepository>();
        var commentReadRepository = Substitute.For<ICommentReadRepository>();
        var complaintWriteRepository = Substitute.For<IComplaintWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        articleReadRepository.ExistsActiveAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        commentReadRepository.GetActiveByIdForArticleAsync(5, 1, Arg.Any<CancellationToken>())
            .Returns(new ActiveCommentDto { Id = 5, UserId = 10, ArticleId = 1 });
        complaintWriteRepository.ExistsForUserAndCommentAsync(2, 5, Arg.Any<CancellationToken>())
            .Returns(true);

        var sut = new SubmitCommentComplaintCommandHandler(
            articleReadRepository,
            commentReadRepository,
            complaintWriteRepository,
            unitOfWork);

        var result = await sut.Handle(
            new SubmitCommentComplaintCommand(1, 5, 2, "Spam"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ErrorCodes.Complaint.AlreadySubmitted);
    }
}
