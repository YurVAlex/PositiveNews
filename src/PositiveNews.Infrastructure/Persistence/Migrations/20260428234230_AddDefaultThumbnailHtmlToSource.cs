using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultThumbnailHtmlToSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultThumbnailHtml",
                schema: "Catalog",
                table: "Sources",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultThumbnailHtml",
                schema: "Catalog",
                table: "Sources");
        }
    }
}
