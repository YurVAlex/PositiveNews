using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.Repositories.Read;
using PositiveNews.Infrastructure.Tests.TestHelpers;

namespace PositiveNews.Infrastructure.Tests.Persistence;

public class CommentReadRepositoryTests
{
    [Fact]
    public async Task GetAdminActiveCommentsAsync_Should_ReturnOnlyActiveComments_OrderedByComplaintCount()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);

        var source = Source.Create("Example Source", "https://example.com");
        ctx.Sources.Add(source);
        var author = EntityBuilders.CreateUser("author@test.com", "Author");
        var complainant = EntityBuilders.CreateUser("complainant@test.com", "Complainant");
        ctx.Users.AddRange(author, complainant);
        await ctx.SaveChangesAsync();

        var article = ArticleMetadata.Create(source.Id, "Title", "https://example.com/a", null, DateTime.UtcNow, "en");
        ctx.ArticlesMetadata.Add(article);
        await ctx.SaveChangesAsync();

        var inactive = Comment.Create(article.Id, author.Id, "Inactive");
        var highComplaints = Comment.Create(article.Id, author.Id, "High complaints");
        var lowComplaints = Comment.Create(article.Id, author.Id, "Low complaints");
        ctx.Comments.AddRange(inactive, highComplaints, lowComplaints);
        await ctx.SaveChangesAsync();

        inactive.SetActive(false, 1);
        ctx.Complains.Add(Complaint.Create(complainant.Id, highComplaints.Id, "Spam"));
        ctx.Complains.Add(Complaint.Create(complainant.Id, highComplaints.Id, "Abuse"));
        await ctx.SaveChangesAsync();

        var sut = new CommentReadRepository(ctx);

        var result = await sut.GetAdminActiveCommentsAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.IsActive);
        result[0].Id.Should().Be(highComplaints.Id);
        result[0].ComplaintCount.Should().Be(2);
        result[1].ComplaintCount.Should().Be(0);
    }
}
