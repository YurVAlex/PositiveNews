using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class CommentTests
{
    [Fact]
    public void Create_Should_BuildActiveComment_When_TopLevel()
    {
        var c = Comment.Create(1, 2, "Hello world");

        c.ArticleId.Should().Be(1);
        c.UserId.Should().Be(2);
        c.Content.Should().Be("Hello world");
        c.ParentId.Should().BeNull();
        c.IsActive.Should().BeTrue();
        c.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_Should_SetParentId_When_Reply()
    {
        var c = Comment.Create(1, 2, "reply", 5);

        c.ParentId.Should().Be(5);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ContentEmpty(string? content)
    {
        var act = () => Comment.Create(1, 2, content!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_ThrowDomainException_When_ContentExceeds2000Characters()
    {
        var longContent = new string('x', 2001);

        var act = () => Comment.Create(1, 2, longContent);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Edit_Should_UpdateContentAndEditedAt_When_ValidInput()
    {
        var c = Comment.Create(1, 2, "Old");

        c.Edit("  New  ");

        c.Content.Should().Be("New");
        c.EditedAt.Should().NotBeNull();
    }

    [Fact]
    public void Edit_Should_ThrowDomainException_When_EmptyOrTooLong()
    {
        var c = Comment.Create(1, 2, "Valid");

        c.Invoking(x => x.Edit("")).Should().Throw<DomainException>();
        c.Invoking(x => x.Edit(new string('x', 2001))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Edit_Should_ThrowDomainException_When_CommentInactive()
    {
        var c = Comment.Create(1, 2, "Valid");
        c.Deactivate(10);

        var act = () => c.Edit("New");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_Should_MarkInactiveAndPreventSecondCall_When_AlreadyInactive()
    {
        var c = Comment.Create(1, 2, "Valid");

        c.Deactivate(10);

        c.IsActive.Should().BeFalse();
        c.ModeratedBy.Should().Be(10);

        var act = () => c.Deactivate(10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SetActive_Should_DeactivateComment_When_SetToFalse()
    {
        var c = Comment.Create(1, 2, "Valid");

        c.SetActive(false, 10);

        c.IsActive.Should().BeFalse();
        c.ModeratedBy.Should().Be(10);
    }

    [Fact]
    public void SetActive_Should_ReactivateComment_When_SetToTrueAfterDeactivate()
    {
        var c = Comment.Create(1, 2, "Valid");
        c.SetActive(false, 10);

        c.SetActive(true, 20);

        c.IsActive.Should().BeTrue();
        c.ModeratedBy.Should().Be(20);
    }

    [Fact]
    public void SetActive_Should_UpdateModerator_When_ValueUnchanged()
    {
        var c = Comment.Create(1, 2, "Valid");

        c.SetActive(true, 15);

        c.IsActive.Should().BeTrue();
        c.ModeratedBy.Should().Be(15);
    }
}
