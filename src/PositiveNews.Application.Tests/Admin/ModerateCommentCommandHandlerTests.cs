using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.CommandHandlers.Admin;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Admin;

public class ModerateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CommentMissing()
    {
        var commentWriteRepository = Substitute.For<ICommentWriteRepository>();
        commentWriteRepository.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns((Comment?)null);
        var handler = new ModerateCommentCommandHandler(
            commentWriteRepository,
            Substitute.For<IAuditLogWriteRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ModerateCommentCommand(123, false, null, null, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Admin.CommentNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_RecordModerationAndSave_When_IsActiveChanges()
    {
        var comment = Comment.Create(1, 2, "Valid comment");
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, 10L);

        var commentWriteRepository = Substitute.For<ICommentWriteRepository>();
        commentWriteRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(comment);
        var auditLogWriteRepository = Substitute.For<IAuditLogWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ModerateCommentCommandHandler(commentWriteRepository, auditLogWriteRepository, unitOfWork);

        var result = await handler.Handle(new ModerateCommentCommand(10, false, "reason", "note", 42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comment.IsActive.Should().BeFalse();
        comment.ModeratedBy.Should().Be(42);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(Comment.IsActive)
            && log.OldValue == true.ToString()
            && log.NewValue == false.ToString()));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationFailure_When_NoChangesProvided()
    {
        var comment = Comment.Create(1, 2, "Valid comment");
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, 11L);

        var commentWriteRepository = Substitute.For<ICommentWriteRepository>();
        commentWriteRepository.GetByIdAsync(11, Arg.Any<CancellationToken>()).Returns(comment);
        var handler = new ModerateCommentCommandHandler(
            commentWriteRepository,
            Substitute.For<IAuditLogWriteRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ModerateCommentCommand(11, true, null, null, 42), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Admin.CommentUnchanged);
    }
}
