using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseImageTagSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageTag",
                schema: "Catalog",
                table: "ArticlesMetadata",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageTag",
                schema: "Catalog",
                table: "ArticlesMetadata",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
