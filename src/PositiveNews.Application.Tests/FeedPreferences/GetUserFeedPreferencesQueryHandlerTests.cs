using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Application.QueryHandlers.FeedPreferences;
using PositiveNews.Application.Queries.FeedPreferences;

namespace PositiveNews.Application.Tests.FeedPreferences;

public class GetUserFeedPreferencesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnDefaults_When_NoRowExists()
    {
        var readRepository = Substitute.For<IUserFeedPreferencesReadRepository>();
        readRepository.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns((UserFeedPreferencesDto?)null);

        var sut = new GetUserFeedPreferencesQueryHandler(readRepository);
        var result = await sut.Handle(new GetUserFeedPreferencesQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TopicNames.Should().BeEmpty();
        result.Value.SourceIds.Should().BeEmpty();
        result.Value.MinPositivity.Should().Be(0.5m);
        result.Value.SortBy.Should().Be("date");
    }

    [Fact]
    public async Task Handle_Should_ReturnStoredPreferences_When_RowExists()
    {
        var stored = new UserFeedPreferencesDto(["Health"], [2], 0.7m, "positivity");
        var readRepository = Substitute.For<IUserFeedPreferencesReadRepository>();
        readRepository.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(stored);

        var sut = new GetUserFeedPreferencesQueryHandler(readRepository);
        var result = await sut.Handle(new GetUserFeedPreferencesQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(stored);
    }
}
