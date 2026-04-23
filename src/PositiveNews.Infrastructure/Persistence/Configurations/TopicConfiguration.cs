using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

internal sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics", SchemaNames.Catalog);

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.Description).HasMaxLength(500);

        // Backing field navigation access for collections
        builder.Navigation(t => t.ArticleTopics)
               .HasField("_articleTopics")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(t => t.UserTopicFilters)
               .HasField("_userTopicFilters")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
