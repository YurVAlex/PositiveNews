using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class UserFeedPreferenceConfiguration : IEntityTypeConfiguration<UserFeedPreference>
{
    public void Configure(EntityTypeBuilder<UserFeedPreference> builder)
    {
        builder.ToTable("UserFeedPreferences", SchemaNames.Identity);

        builder.HasKey(p => p.UserId);
        builder.Property(p => p.MinPositivity).HasColumnType("decimal(3,2)").HasDefaultValue(0.5m);
        builder.Property(p => p.SortBy).HasMaxLength(50).HasDefaultValue("Date");
        builder.Property(p => p.LanguageCode).HasMaxLength(10);
        builder.Property(p => p.RegionCode).HasMaxLength(10);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_UserPrefs_MinPositivity", "[MinPositivity] BETWEEN 0.00 AND 1.00"));

        builder.HasOne(p => p.User)
               .WithOne(u => u.FeedPreference)
               .HasForeignKey<UserFeedPreference>(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}