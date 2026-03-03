using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments", SchemaNames.Community);

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        builder.HasOne(c => c.Article)
               .WithMany(a => a.Comments)
               .HasForeignKey(c => c.ArticleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany(u => u.Comments)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Replies)
               .HasForeignKey(c => c.ParentId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Moderator)
               .WithMany()
               .HasForeignKey(c => c.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);

        // Thread loading index
        builder.HasIndex(c => new { c.ArticleId, c.CreatedAt })
               .HasFilter("[IsActive] = 1")
               .HasDatabaseName("IX_Comments_Article_Thread");
    }
}