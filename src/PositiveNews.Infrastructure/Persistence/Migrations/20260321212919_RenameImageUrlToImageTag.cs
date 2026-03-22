using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageUrlToImageTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "Catalog",
                table: "ArticlesMetadata");

            migrationBuilder.AddColumn<string>(
                name: "ImageTag",
                schema: "Catalog",
                table: "ArticlesMetadata",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageTag",
                schema: "Catalog",
                table: "ArticlesMetadata");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "Catalog",
                table: "ArticlesMetadata",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
