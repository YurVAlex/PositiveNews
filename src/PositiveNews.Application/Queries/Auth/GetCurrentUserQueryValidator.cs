using FluentValidation;

namespace PositiveNews.Application.Queries.Auth;

public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
