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

        if (article.IsActive == request.IsActive)
        {
            return Result.Failure(new Error(
                "Admin.ArticleUnchanged",
                "No moderation change was provided.",
                ErrorType.Validation));
        }

        var oldValue = article.IsActive.ToString();
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
            oldValue,
            article.IsActive.ToString(),
            request.Reason,
            request.Note));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
