using FluentValidation;
using PositiveNews.Domain.Constants;

namespace PositiveNews.Application.Commands.Comments;

/// <summary>
/// Validates <see cref="AddArticleCommentCommand"/> payload.
/// </summary>
public sealed class AddArticleCommentCommandValidator : AbstractValidator<AddArticleCommentCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="AddArticleCommentCommand"/>.
    /// </summary>
    public AddArticleCommentCommandValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Content)
            .NotEmpty()
            .Must(content => !string.IsNullOrWhiteSpace(content)).WithMessage("Comment content cannot be empty.")
            .Must(content => content.Trim().Length <= FieldLengths.Comment.Content)
            .WithMessage($"Comment content cannot exceed {FieldLengths.Comment.Content} characters.");
    }
}