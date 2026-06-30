using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMinPositivityDefaultToZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinPositivity",
                schema: "Identity",
                table: "UserFeedPreferences",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldDefaultValue: 0.5m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinPositivity",
                schema: "Identity",
                table: "UserFeedPreferences",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.5m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldDefaultValue: 0m);
        }
    }
}
