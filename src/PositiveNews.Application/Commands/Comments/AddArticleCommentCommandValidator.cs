using FluentValidation;

namespace PositiveNews.Application.Commands.Comments;

/// <summary>
/// Validates <see cref="AddArticleCommentCommand"/> payload.
/// </summary>
public sealed class AddArticleCommentCommandValidator : AbstractValidator<AddArticleCommentCommand>
{
    private const int MaxContentLength = 2000;

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
            .Must(content => content.Trim().Length <= MaxContentLength)
            .WithMessage($"Comment content cannot exceed {MaxContentLength} characters.");
    }
}
