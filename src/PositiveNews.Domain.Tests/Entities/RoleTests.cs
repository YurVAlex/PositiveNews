using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class RoleTests
{
    [Fact]
    public void Create_Should_TrimName_When_NameHasPadding()
    {
        var role = Role.Create("  Admin  ");

        role.Name.Should().Be("Admin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_NameIsNullOrWhitespace(string? name)
    {
        var act = () => Role.Create(name!);

        act.Should().Throw<DomainException>();
    }
}
