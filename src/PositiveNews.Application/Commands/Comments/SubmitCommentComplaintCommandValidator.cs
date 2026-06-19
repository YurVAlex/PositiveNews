using FluentValidation;

namespace PositiveNews.Application.Commands.Comments;

/// <summary>
/// Validates <see cref="SubmitCommentComplaintCommand"/> payload.
/// </summary>
public sealed class SubmitCommentComplaintCommandValidator : AbstractValidator<SubmitCommentComplaintCommand>
{
    private const int MaxReasonLength = 500;

    /// <summary>
    /// Initializes validation rules for <see cref="SubmitCommentComplaintCommand"/>.
    /// </summary>
    public SubmitCommentComplaintCommandValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
        RuleFor(x => x.CommentId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Complaint reason cannot be empty.")
            .Must(reason => reason.Trim().Length <= MaxReasonLength)
            .WithMessage($"Complaint reason cannot exceed {MaxReasonLength} characters.");
    }
}
