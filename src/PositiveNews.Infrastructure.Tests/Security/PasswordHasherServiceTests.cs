using FluentAssertions;
using PositiveNews.Infrastructure.Security;
using PositiveNews.Infrastructure.Tests.TestHelpers;

namespace PositiveNews.Infrastructure.Tests.Security;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _sut = new();

    [Fact]
    public void HashPassword_Should_NotEqualPlainText_When_PasswordProvided()
    {
        var user = EntityBuilders.CreateUser();

        var hash = _sut.HashPassword(user, "Secret1!");

        hash.Should().NotBe("Secret1!");
        hash.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void VerifyPassword_Should_ReturnTrue_When_PasswordMatchesHash()
    {
        var user = EntityBuilders.CreateUser();
        var hash = _sut.HashPassword(user, "Secret1!");

        var ok = _sut.VerifyPassword(user, hash, "Secret1!");

        ok.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_When_PasswordWrong()
    {
        var user = EntityBuilders.CreateUser();
        var hash = _sut.HashPassword(user, "Secret1!");

        var ok = _sut.VerifyPassword(user, hash, "Other!");

        ok.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_Should_ProduceDifferentHashes_When_SamePasswordHashedTwice()
    {
        var user = EntityBuilders.CreateUser();

        var h1 = _sut.HashPassword(user, "Secret1!");
        var h2 = _sut.HashPassword(user, "Secret1!");

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void VerifyPassword_Should_HandleEmptyPassword_When_HashExists()
    {
        var user = EntityBuilders.CreateUser();
        var hash = _sut.HashPassword(user, "");

        var ok = _sut.VerifyPassword(user, hash, "");

        ok.Should().BeTrue();
    }
}
