using FluentValidation;

namespace PositiveNews.Application.Commands.Admin;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User id must be a positive integer.");

        RuleFor(x => x.Reason)
            .MaximumLength(256).WithMessage("Reason must be 256 characters or fewer.");

        RuleFor(x => x.Note)
            .MaximumLength(1024).WithMessage("Note must be 1024 characters or fewer.");

        RuleFor(x => x.ModeratorId)
            .GreaterThan(0).WithMessage("ModeratorId must be a valid user identifier.");
    }
}