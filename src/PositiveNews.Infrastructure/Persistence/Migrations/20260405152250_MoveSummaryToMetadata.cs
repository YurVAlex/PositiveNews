using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveSummaryToMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryShort",
                schema: "Catalog",
                table: "ArticlesContent");

            migrationBuilder.AddColumn<string>(
                name: "SummaryShort",
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
                name: "SummaryShort",
                schema: "Catalog",
                table: "ArticlesMetadata");

            migrationBuilder.AddColumn<string>(
                name: "SummaryShort",
                schema: "Catalog",
                table: "ArticlesContent",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
