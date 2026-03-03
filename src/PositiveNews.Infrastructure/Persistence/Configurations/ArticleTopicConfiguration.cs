using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class ArticleTopicConfiguration : IEntityTypeConfiguration<ArticleTopic>
{
    public void Configure(EntityTypeBuilder<ArticleTopic> builder)
    {
        builder.ToTable("ArticleTopics", SchemaNames.Catalog);

        builder.HasKey(at => new { at.ArticleId, at.TopicId });

        builder.HasOne(at => at.Article)
               .WithMany(a => a.ArticleTopics)
               .HasForeignKey(at => at.ArticleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(at => at.Topic)
               .WithMany(t => t.ArticleTopics)
               .HasForeignKey(at => at.TopicId)
               .OnDelete(DeleteBehavior.Cascade);

        // Reverse lookup index
        builder.HasIndex(at => at.TopicId)
               .HasDatabaseName("IX_ArticleTopics_Topic_Lookup");
    }
}