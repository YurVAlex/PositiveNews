using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class ArticleContentConfiguration : IEntityTypeConfiguration<ArticleContent>
{
    public void Configure(EntityTypeBuilder<ArticleContent> builder)
    {
        builder.ToTable("ArticlesContent", SchemaNames.Catalog);

        builder.HasKey(c => c.Id);

        // Id is NOT identity — it shares the value from ArticleMetadata.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ContentRaw).HasColumnType("nvarchar(max)");
        builder.Property(c => c.ContentClean).HasColumnType("nvarchar(max)");
        builder.Property(c => c.SummaryShort).HasMaxLength(2000);
    }
}