using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Application.CommandHandlers.Admin;

/// <summary>
/// Applies admin edits to a source and records audit logs.
/// </summary>
public sealed class UpdateSourceCommandHandler(
    ISourceWriteRepository sourceWriteRepository,
    IArticleWriteRepository articleWriteRepository,
    IAuditLogWriteRepository auditLogWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSourceCommand, Result>
{
    public async Task<Result> Handle(UpdateSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await sourceWriteRepository.GetByIdAsync(request.SourceId, cancellationToken);
        if (source is null)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.SourceNotFound,
                $"Source with id '{request.SourceId}' was not found.",
                ErrorType.NotFound));
        }

        var changed = false;

        if (source.TrustScore != request.TrustScore)
        {
            var oldValue = source.TrustScore.ToString("G29");
            source.SetTrustScore(request.TrustScore, request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Source,
                source.Id,
                request.ModeratorId,
                nameof(Source.TrustScore),
                oldValue,
                source.TrustScore.ToString("G29"),
                request.Reason,
                request.Note));
            changed = true;
        }

        if (source.IsActive != request.IsActive)
        {
            var oldValue = source.IsActive.ToString();
            source.SetActive(request.IsActive, request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Source,
                source.Id,
                request.ModeratorId,
                nameof(Source.IsActive),
                oldValue,
                source.IsActive.ToString(),
                request.Reason,
                request.Note));

            if (request.IsActive)
            {
                await articleWriteRepository.ActivateBySourceAsync(source.Id, request.ModeratorId, cancellationToken);
            }
            else
            {
                await articleWriteRepository.DeactivateBySourceAsync(source.Id, request.ModeratorId, cancellationToken);
            }

            changed = true;
        }

        if (source.FeedUrl != request.FeedUrl)
        {
            var oldValue = source.FeedUrl ?? string.Empty;
            source.UpdateFeedUrl(request.FeedUrl);
            source.ApplyModeration(request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Source,
                source.Id,
                request.ModeratorId,
                nameof(Source.FeedUrl),
                oldValue,
                source.FeedUrl ?? string.Empty,
                request.Reason,
                request.Note));
            changed = true;
        }

        if (!changed)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.SourceUnchanged,
                "No changes were provided.",
                ErrorType.Validation));
        }

        if (!source.IsActive)
        {
            await articleWriteRepository.DeactivateBySourceAsync(source.Id, request.ModeratorId, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
