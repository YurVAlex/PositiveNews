using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="UserTopicFilter"/>.
/// </summary>
internal sealed class UserTopicFilterConfiguration : IEntityTypeConfiguration<UserTopicFilter>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserTopicFilter> builder)
    {
        builder.ToTable("UserTopicFilters", SchemaNames.Identity);

        builder.HasKey(utf => new { utf.UserId, utf.TopicId });

        builder.HasOne(utf => utf.User)
               .WithMany(u => u.TopicFilters)
               .HasForeignKey(utf => utf.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utf => utf.Topic)
               .WithMany(t => t.UserTopicFilters)
               .HasForeignKey(utf => utf.TopicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
