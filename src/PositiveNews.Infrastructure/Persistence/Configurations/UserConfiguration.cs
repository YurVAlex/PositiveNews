using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="User"/>.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", SchemaNames.Identity);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(FieldLengths.User.Email).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.Name).HasMaxLength(FieldLengths.User.Name).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnType("nvarchar(max)");
        builder.Property(u => u.FailedLoginCount).HasDefaultValue(0);
        builder.Property(u => u.AvatarPictureUrl).HasMaxLength(FieldLengths.User.AvatarUrl);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(u => u.IsActive).HasDefaultValue(true);

        builder.HasOne(u => u.Moderator)
               .WithMany()
               .HasForeignKey(u => u.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);

        // Backing field navigation access for collections
        builder.Navigation(u => u.UserRoles)
               .HasField("_userRoles")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.SourceFilters)
               .HasField("_sourceFilters")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.TopicFilters)
               .HasField("_topicFilters")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.Comments)
               .HasField("_comments")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
