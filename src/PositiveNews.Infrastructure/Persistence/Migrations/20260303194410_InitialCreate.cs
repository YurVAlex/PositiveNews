using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PositiveNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catalog");

            migrationBuilder.EnsureSchema(
                name: "Admin");

            migrationBuilder.EnsureSchema(
                name: "Community");

            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AvatarPictureUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModeratedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "Admin",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    ChangedField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ModeratorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.CheckConstraint("CK_Audit_Entity", "[EntityType] IN ('Article', 'Comment', 'User', 'Source')");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ModeratorId",
                        column: x => x.ModeratorId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FeedUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiEncryptedKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrustScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1.0m),
                    DefaultLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModeratedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                    table.CheckConstraint("CK_Sources_Trust", "[TrustScore] >= 0.00");
                    table.ForeignKey(
                        name: "FK_Sources_Users_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserFeedPreferences",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MinPositivity = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.5m),
                    SortBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Date"),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeedPreferences", x => x.UserId);
                    table.CheckConstraint("CK_UserPrefs_MinPositivity", "[MinPositivity] BETWEEN 0.00 AND 1.00");
                    table.ForeignKey(
                        name: "FK_UserFeedPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTopicFilters",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopicFilters", x => new { x.UserId, x.TopicId });
                    table.ForeignKey(
                        name: "FK_UserTopicFilters_Topics_TopicId",
                        column: x => x.TopicId,
                        principalSchema: "Catalog",
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTopicFilters_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticlesMetadata",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    IngestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PositivityScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "und"),
                    RegionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Global"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModeratedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlesMetadata", x => x.Id);
                    table.CheckConstraint("CK_Articles_Positivity", "[PositivityScore] BETWEEN 0.0000 AND 1.0000");
                    table.ForeignKey(
                        name: "FK_ArticlesMetadata_Sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Catalog",
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticlesMetadata_Users_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IngestionRuns",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemsFetched = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionRuns", x => x.Id);
                    table.CheckConstraint("CK_Ingestion_Status", "[Status] IN ('Running', 'Success', 'Failed', 'Partial')");
                    table.ForeignKey(
                        name: "FK_IngestionRuns_Sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Catalog",
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSourceFilters",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSourceFilters", x => new { x.UserId, x.SourceId });
                    table.ForeignKey(
                        name: "FK_UserSourceFilters_Sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Catalog",
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSourceFilters_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticlesContent",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ContentRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentClean = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryShort = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlesContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticlesContent_ArticlesMetadata_Id",
                        column: x => x.Id,
                        principalSchema: "Catalog",
                        principalTable: "ArticlesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTopics",
                schema: "Catalog",
                columns: table => new
                {
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTopics", x => new { x.ArticleId, x.TopicId });
                    table.ForeignKey(
                        name: "FK_ArticleTopics_ArticlesMetadata_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "Catalog",
                        principalTable: "ArticlesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalSchema: "Catalog",
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "Community",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModeratedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_ArticlesMetadata_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "Catalog",
                        principalTable: "ArticlesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Community",
                        principalTable: "Comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Users_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesMeta_Feed_Date",
                schema: "Catalog",
                table: "ArticlesMetadata",
                columns: new[] { "LanguageCode", "RegionCode", "PublishedAt" },
                descending: new[] { false, false, true },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesMeta_Source_ExternalId",
                schema: "Catalog",
                table: "ArticlesMetadata",
                columns: new[] { "SourceId", "ExternalId" },
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesMeta_SourceId",
                schema: "Catalog",
                table: "ArticlesMetadata",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesMetadata_ModeratedBy",
                schema: "Catalog",
                table: "ArticlesMetadata",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTopics_Topic_Lookup",
                schema: "Catalog",
                table: "ArticleTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity_History",
                schema: "Admin",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ModeratorId",
                schema: "Admin",
                table: "AuditLogs",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Article_Thread",
                schema: "Community",
                table: "Comments",
                columns: new[] { "ArticleId", "CreatedAt" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ModeratedBy",
                schema: "Community",
                table: "Comments",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentId",
                schema: "Community",
                table: "Comments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                schema: "Community",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionRuns_SourceId",
                schema: "Catalog",
                table: "IngestionRuns",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "Identity",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sources_ModeratedBy",
                schema: "Catalog",
                table: "Sources",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Slug",
                schema: "Catalog",
                table: "Topics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "Identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "Identity",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModeratedBy",
                schema: "Identity",
                table: "Users",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserSourceFilters_SourceId",
                schema: "Identity",
                table: "UserSourceFilters",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicFilters_TopicId",
                schema: "Identity",
                table: "UserTopicFilters",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticlesContent",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "ArticleTopics",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "Admin");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "Community");

            migrationBuilder.DropTable(
                name: "IngestionRuns",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "UserFeedPreferences",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserSourceFilters",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserTopicFilters",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "ArticlesMetadata",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Topics",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Sources",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Identity");
        }
    }
}
