using FluentValidation;
using PositiveNews.Application.Commands.Admin;

namespace PositiveNews.Application.Commands.Admin;

public sealed class UpdateSourceCommandValidator : AbstractValidator<UpdateSourceCommand>
{
    public UpdateSourceCommandValidator()
    {
        RuleFor(x => x.SourceId)
            .GreaterThan(0).WithMessage("Source id must be a positive integer.");

        RuleFor(x => x.TrustScore)
            .GreaterThanOrEqualTo(0m).WithMessage("Trust score must be 0 or greater.");

        RuleFor(x => x.FeedUrl)
            .NotEmpty().WithMessage("Feed URL is required.")
            .MaximumLength(1024).WithMessage("Feed URL must be at most 1024 characters.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Feed URL must be a valid absolute HTTP or HTTPS URL.");

        RuleFor(x => x.Reason)
            .MaximumLength(256).WithMessage("Reason must be 256 characters or fewer.");

        RuleFor(x => x.Note)
            .MaximumLength(1024).WithMessage("Note must be 1024 characters or fewer.");

        RuleFor(x => x.ModeratorId)
            .GreaterThan(0).WithMessage("ModeratorId must be a valid user identifier.");
    }
}
