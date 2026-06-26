using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveComplaintArticleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complains_ArticlesMetadata_ArticleId",
                schema: "Community",
                table: "Complains");

            migrationBuilder.DropIndex(
                name: "IX_Complains_ArticleId",
                schema: "Community",
                table: "Complains");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                schema: "Community",
                table: "Complains");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ArticleId",
                schema: "Community",
                table: "Complains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Complains_ArticleId",
                schema: "Community",
                table: "Complains",
                column: "ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complains_ArticlesMetadata_ArticleId",
                schema: "Community",
                table: "Complains",
                column: "ArticleId",
                principalSchema: "Catalog",
                principalTable: "ArticlesMetadata",
                principalColumn: "Id");
        }
    }
}
