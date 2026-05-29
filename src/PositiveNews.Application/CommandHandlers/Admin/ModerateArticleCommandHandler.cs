using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Application.CommandHandlers.Admin;

/// <summary>
/// Applies moderation actions to article state and records audit events.
/// </summary>
public sealed class ModerateArticleCommandHandler(
    IArticleWriteRepository articleWriteRepository,
    IAuditLogWriteRepository auditLogWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ModerateArticleCommand, Result>
{
    public async Task<Result> Handle(ModerateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articleWriteRepository.GetByIdAsync(request.ArticleId, cancellationToken);
        if (article is null)
        {
            return Result.Failure(new Error(
                "Admin.ArticleNotFound",
                $"Article with id '{request.ArticleId}' was not found.",
                ErrorType.NotFound));
        }

        var hasChange = false;
        var activeChanged = article.IsActive != request.IsActive;
        var oldActiveValue = article.IsActive.ToString();

        if (activeChanged)
        {
            if (request.IsActive)
            {
                article.Activate(request.ModeratorId);
            }
            else
            {
                article.Deactivate(request.ModeratorId);
            }

            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Article,
                article.Id,
                request.ModeratorId,
                nameof(ArticleMetadata.IsActive),
                oldActiveValue,
                article.IsActive.ToString(),
                request.Reason,
                request.Note));

            hasChange = true;
        }

        if (request.Title is not null && request.Title.Trim() != article.Title)
        {
            var oldValue = article.Title;
            article.UpdateTitle(request.Title);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Article,
                article.Id,
                request.ModeratorId,
                nameof(ArticleMetadata.Title),
                oldValue,
                article.Title,
                request.Reason,
                request.Note));
            hasChange = true;
        }

        if (request.ImageTag is not null && request.ImageTag != article.ImageTag)
        {
            var oldValue = article.ImageTag ?? string.Empty;
            article.UpdateImageTag(request.ImageTag);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Article,
                article.Id,
                request.ModeratorId,
                nameof(ArticleMetadata.ImageTag),
                oldValue,
                article.ImageTag ?? string.Empty,
                request.Reason,
                request.Note));
            hasChange = true;
        }

        if (request.PositivityScore != article.PositivityScore)
        {
            var oldValue = article.PositivityScore?.ToString() ?? string.Empty;
            article.UpdatePositivityScore(request.PositivityScore);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Article,
                article.Id,
                request.ModeratorId,
                nameof(ArticleMetadata.PositivityScore),
                oldValue,
                request.PositivityScore?.ToString() ?? string.Empty,
                request.Reason,
                request.Note));
            hasChange = true;
        }

        if (request.SummaryShort is not null && request.SummaryShort != article.SummaryShort)
        {
            var oldValue = article.SummaryShort ?? string.Empty;
            article.UpdateSummaryShort(request.SummaryShort);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Article,
                article.Id,
                request.ModeratorId,
                nameof(ArticleMetadata.SummaryShort),
                oldValue,
                article.SummaryShort ?? string.Empty,
                request.Reason,
                request.Note));
            hasChange = true;
        }

        if (request.ContentRaw is not null)
        {
            if (article.Content is null)
            {
                article.AttachContent(ArticleContent.Create(request.ContentRaw, null));
                auditLogWriteRepository.Add(AuditLog.Create(
                    AuditEntityType.Article,
                    article.Id,
                    request.ModeratorId,
                    nameof(ArticleContent.ContentRaw),
                    string.Empty,
                    request.ContentRaw,
                    request.Reason,
                    request.Note));
                hasChange = true;
            }
            else if (request.ContentRaw != article.Content.ContentRaw)
            {
                var oldValue = article.Content.ContentRaw ?? string.Empty;
                article.Content.UpdateContent(request.ContentRaw, article.Content.ContentClean);
                auditLogWriteRepository.Add(AuditLog.Create(
                    AuditEntityType.Article,
                    article.Id,
                    request.ModeratorId,
                    nameof(ArticleContent.ContentRaw),
                    oldValue,
                    request.ContentRaw,
                    request.Reason,
                    request.Note));
                hasChange = true;
            }
        }

        if (!hasChange)
        {
            return Result.Failure(new Error(
                "Admin.ArticleUnchanged",
                "No moderation or metadata changes were provided.",
                ErrorType.Validation));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
