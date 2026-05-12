using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_Should_NormalizeEmailAndNameAndSetDefaults_When_InputHasWhitespaceAndMixedCase()
    {
        var user = User.Create("  USER@Example.COM  ", "  Jane  ");

        user.Email.Should().Be("user@example.com");
        user.Name.Should().Be("Jane");
        user.EmailConfirmed.Should().BeFalse();
        user.FailedLoginCount.Should().Be(0);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidUserStateException_When_EmailIsInvalid(string? email)
    {
        var act = () => User.Create(email!, "Name");

        act.Should().Throw<InvalidUserStateException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidUserStateException_When_NameIsInvalid(string? name)
    {
        var act = () => User.Create("a@b.com", name!);

        act.Should().Throw<InvalidUserStateException>();
    }

    [Fact]
    public void ConfirmEmail_Should_SetEmailConfirmed_When_Called()
    {
        var user = User.Create("a@b.com", "Name");

        user.ConfirmEmail();

        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ChangeEmail_Should_UpdateEmailAndResetConfirmation_When_NewEmailProvided()
    {
        var user = User.Create("old@b.com", "Name");
        user.ConfirmEmail();

        user.ChangeEmail("  New@B.COM  ");

        user.Email.Should().Be("new@b.com");
        user.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void ChangeEmail_Should_ThrowInvalidUserStateException_When_NewEmailEmpty()
    {
        var user = User.Create("a@b.com", "Name");

        var act = () => user.ChangeEmail("");

        act.Should().Throw<InvalidUserStateException>();
    }

    [Fact]
    public void SetPasswordHash_Should_StoreAndClearHash_When_SetAndCleared()
    {
        var user = User.Create("a@b.com", "Name");

        user.SetPasswordHash("hashed");
        user.PasswordHash.Should().Be("hashed");

        user.SetPasswordHash(null);

        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void SetAvatarUrl_Should_TrimUrl_When_InputHasWhitespace()
    {
        var user = User.Create("a@b.com", "Name");

        user.SetAvatarUrl("  http://pic  ");

        user.AvatarPictureUrl.Should().Be("http://pic");
    }

    [Fact]
    public void RecordFailedLoginAndRecordSuccessfulLogin_Should_ResetFailuresAndSetLastLogin_When_SuccessAfterFailures()
    {
        var user = User.Create("a@b.com", "Name");

        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.FailedLoginCount.Should().Be(2);

        user.RecordSuccessfulLogin();

        user.FailedLoginCount.Should().Be(0);
        user.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Should_MarkInactiveAndPreventSecondCall_When_AlreadyDeactivated()
    {
        var user = User.Create("a@b.com", "Name");

        user.Deactivate(42);

        user.IsActive.Should().BeFalse();
        user.ModeratedBy.Should().Be(42);

        var act = () => user.Deactivate(42);

        act.Should().Throw<InvalidUserStateException>();
    }
}
