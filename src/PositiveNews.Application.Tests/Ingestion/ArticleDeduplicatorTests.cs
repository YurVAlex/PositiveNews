using FluentAssertions;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Services.Ingestion;

namespace PositiveNews.Application.Tests.Ingestion;

public class ArticleDeduplicatorTests
{
    private readonly ArticleDeduplicator _sut = new();

    [Fact]
    public void MatchesExisting_Should_ReturnTrue_When_ExternalIdMatchesPersistedKey()
    {
        var keys = new ExistingArticleKeys(
            new HashSet<string>(["ext-1"]),
            new HashSet<string>(),
            new HashSet<string>());
        var dto = new RssFeedItemDto { ExternalId = "ext-1", Link = "https://new", Title = "New" };

        _sut.MatchesExisting(keys, dto).Should().BeTrue();
    }

    [Fact]
    public void MatchesExisting_Should_ReturnTrue_When_UrlMatchesPersistedKey()
    {
        var keys = new ExistingArticleKeys(
            new HashSet<string>(),
            new HashSet<string>(["https://example.com/a"]),
            new HashSet<string>());
        var dto = new RssFeedItemDto { ExternalId = "", Link = "https://example.com/a", Title = "Any" };

        _sut.MatchesExisting(keys, dto).Should().BeTrue();
    }

    [Fact]
    public void MatchesExisting_Should_ReturnTrue_When_TitleMatchesPersistedKey()
    {
        var keys = new ExistingArticleKeys(
            new HashSet<string>(),
            new HashSet<string>(),
            new HashSet<string>(["Existing title"]));
        var dto = new RssFeedItemDto { ExternalId = "", Link = "new", Title = "Existing title" };

        _sut.MatchesExisting(keys, dto).Should().BeTrue();
    }

    [Fact]
    public void MatchesExisting_Should_ReturnFalse_When_NoKeyMatchesNewItem()
    {
        var keys = new ExistingArticleKeys(
            new HashSet<string>(["ext-1"]),
            new HashSet<string>(["https://example.com/a"]),
            new HashSet<string>(["Existing title"]));
        var dto = new RssFeedItemDto { ExternalId = "", Link = "https://new", Title = "New title" };

        _sut.MatchesExisting(keys, dto).Should().BeFalse();
    }

    [Fact]
    public void ConflictsWithPending_Should_ReturnFalse_When_EmptyExternalIdIgnoredAgainstEmptyPendingSet()
    {
        var dto = new RssFeedItemDto { ExternalId = "", Link = "new", Title = "New" };

        _sut.ConflictsWithPending(
                dto,
                new HashSet<string>([""]),
                new HashSet<string>(["pending-url"]),
                new HashSet<string>(["Pending title"]))
            .Should().BeFalse();
    }

    [Fact]
    public void ConflictsWithPending_Should_ReturnTrue_When_LinkAlreadyPending()
    {
        var dto = new RssFeedItemDto { Link = "pending-url", Title = "Different title" };

        _sut.ConflictsWithPending(
                dto,
                [],
                new HashSet<string>(["pending-url"]),
                [])
            .Should().BeTrue();
    }

    [Fact]
    public void ConflictsWithPending_Should_ReturnTrue_When_ExternalIdAlreadyPending()
    {
        var dto = new RssFeedItemDto { ExternalId = "guid-1", Link = "https://a.com", Title = "T1" };

        _sut.ConflictsWithPending(
                dto,
            new HashSet<string>(["guid-1"]),
                new HashSet<string>(),
                new HashSet<string>())
            .Should().BeTrue();
    }
}
