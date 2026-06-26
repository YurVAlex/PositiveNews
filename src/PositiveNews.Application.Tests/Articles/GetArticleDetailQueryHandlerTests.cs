using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Application.QueryHandlers.Articles;

namespace PositiveNews.Application.Tests.Articles;

public class GetArticleDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_IncrementViewCountAndSave_When_ArticleExists()
    {
        var readRepository = Substitute.For<IArticleReadRepository>();
        var writeRepository = Substitute.For<IArticleWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var detail = new ArticleDetailDto { Id = 7, Title = "Story" };

        readRepository.GetDetailAsync(7, Arg.Any<CancellationToken>()).Returns(detail);
        writeRepository.TryIncrementViewCountAsync(7, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new GetArticleDetailQueryHandler(readRepository, writeRepository, unitOfWork);

        var result = await sut.Handle(new GetArticleDetailQuery(7), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(detail);
        await writeRepository.Received(1).TryIncrementViewCountAsync(7, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotSave_When_ArticleNotFound()
    {
        var readRepository = Substitute.For<IArticleReadRepository>();
        var writeRepository = Substitute.For<IArticleWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        readRepository.GetDetailAsync(99, Arg.Any<CancellationToken>()).Returns((ArticleDetailDto?)null);

        var sut = new GetArticleDetailQueryHandler(readRepository, writeRepository, unitOfWork);

        var result = await sut.Handle(new GetArticleDetailQuery(99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await writeRepository.DidNotReceive().TryIncrementViewCountAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
