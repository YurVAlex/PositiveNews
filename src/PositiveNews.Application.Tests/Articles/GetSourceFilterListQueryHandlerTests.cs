using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Application.QueryHandlers.Articles;

namespace PositiveNews.Application.Tests.Articles;

public class GetSourceFilterListQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnSources_FromRepository()
    {
        IReadOnlyList<SourceFilterItemDto> expected =
        [
            new() { Id = 3, Name = "Gamma", LogoUrl = null }
        ];

        var sourceReadRepository = Substitute.For<ISourceReadRepository>();
        sourceReadRepository
            .GetSourceFilterListAsync(Arg.Any<CancellationToken>())
            .Returns(expected);

        var sut = new GetSourceFilterListQueryHandler(sourceReadRepository);

        var result = await sut.Handle(new GetSourceFilterListQuery(), CancellationToken.None);

        result.Should().BeSameAs(expected);
        await sourceReadRepository.Received(1).GetSourceFilterListAsync(Arg.Any<CancellationToken>());
    }
}
