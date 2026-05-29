using FluentValidation;

namespace PositiveNews.Application.Commands.Admin;

public sealed class ModerateArticleCommandValidator : AbstractValidator<ModerateArticleCommand>
{
    public ModerateArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .GreaterThan(0).WithMessage("Article id must be a positive integer.");

        RuleFor(x => x.ModeratorId)
            .GreaterThan(0).WithMessage("ModeratorId must be a valid user identifier.");

        RuleFor(x => x.Title)
            .NotEmpty().When(x => x.Title is not null).WithMessage("Title cannot be empty when provided.")
            .MaximumLength(500).WithMessage("Title must be 500 characters or fewer.");

        RuleFor(x => x.ImageTag)
            .MaximumLength(2048).WithMessage("Image tag must be 2048 characters or fewer.");

        RuleFor(x => x.PositivityScore)
            .InclusiveBetween(0m, 1m).When(x => x.PositivityScore.HasValue)
            .WithMessage("Positivity score must be between 0 and 1.");

        RuleFor(x => x.SummaryShort)
            .MaximumLength(1024).WithMessage("Summary short must be 1024 characters or fewer.");

        RuleFor(x => x.ContentRaw)
            .MaximumLength(20000).WithMessage("Content raw must be 20000 characters or fewer.");

        RuleFor(x => x.Reason)
            .MaximumLength(256).WithMessage("Reason must be 256 characters or fewer.");

        RuleFor(x => x.Note)
            .MaximumLength(1024).WithMessage("Note must be 1024 characters or fewer.");
    }
}
