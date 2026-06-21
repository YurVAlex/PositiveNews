using FluentValidation;

namespace PositiveNews.Application.Queries.Admin;

public sealed class GetAdminCommentDetailQueryValidator : AbstractValidator<GetAdminCommentDetailQuery>
{
    public GetAdminCommentDetailQueryValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("Comment id must be a positive integer.");
    }
}
