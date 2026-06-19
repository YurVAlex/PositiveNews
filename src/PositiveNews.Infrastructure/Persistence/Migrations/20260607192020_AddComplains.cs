using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Complains",
                schema: "Community",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    CommentId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complains_ArticlesMetadata_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "Catalog",
                        principalTable: "ArticlesMetadata",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Complains_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "Community",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Complains_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complains_ArticleId",
                schema: "Community",
                table: "Complains",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Complains_CommentId",
                schema: "Community",
                table: "Complains",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Complains_User_Comment",
                schema: "Community",
                table: "Complains",
                columns: new[] { "UserId", "CommentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Complains",
                schema: "Community");
        }
    }
}
