using FluentValidation;

namespace PositiveNews.Application.Queries.Auth;

/// <summary>
/// Requires a positive user identifier.
/// </summary>
public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    /// <summary>
    /// Initializes validation rules for <see cref="GetCurrentUserQuery"/>.
    /// </summary>
    public GetCurrentUserQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
