using FluentValidation;
using PositiveNews.Application.Mapping;

namespace PositiveNews.Application.Commands.FeedPreferences;

/// <summary>
/// Validates inbound feed preference snapshots before persistence.
/// </summary>
public sealed class UpdateUserFeedPreferencesCommandValidator : AbstractValidator<UpdateUserFeedPreferencesCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="UpdateUserFeedPreferencesCommand"/>.
    /// </summary>
    public UpdateUserFeedPreferencesCommandValidator()
    {
        RuleFor(c => c.UserId).GreaterThan(0);
        RuleFor(c => c.MinPositivity).InclusiveBetween(0m, 1m);
        RuleFor(c => c.SortBy)
            .Must(FeedPreferenceSortMapper.IsValidApiSort)
            .WithMessage("SortBy must be one of: date, positivity, preferences.");
        RuleForEach(c => c.TopicNames)
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("Topic names cannot be empty.");
        RuleForEach(c => c.SourceIds)
            .GreaterThan(0);
    }
}
