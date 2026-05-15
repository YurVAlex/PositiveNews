using FluentAssertions;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.Tests.Validation;

public class GetArticleFeedQueryValidatorTests
{
    private readonly GetArticleFeedQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_PageLessThanOne()
    {
        var result = _validator.Validate(new GetArticleFeedQuery(Page: 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetArticleFeedQuery.Page));
    }

    [Fact]
    public void Validate_Should_Fail_When_TopicIsWhitespace()
    {
        var result = _validator.Validate(new GetArticleFeedQuery(Page: 1, Topics: ["   "]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Topics[0]" && e.ErrorMessage == "Topic filters cannot be empty.");
    }

    [Fact]
    public void Validate_Should_Fail_When_SortByIsUndefined()
    {
        var result = _validator.Validate(new GetArticleFeedQuery(Page: 1, SortBy: (ArticleFeedSortBy)(-1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetArticleFeedQuery.SortBy));
    }

    [Fact]
    public void Validate_Should_Succeed_When_RequestIsValid()
    {
        var result = _validator.Validate(
            new GetArticleFeedQuery(
                Page: 1,
                Topics: ["Space"],
                PageSize: 10,
                SortBy: ArticleFeedSortBy.PositivityScore));

        result.IsValid.Should().BeTrue();
    }
}
