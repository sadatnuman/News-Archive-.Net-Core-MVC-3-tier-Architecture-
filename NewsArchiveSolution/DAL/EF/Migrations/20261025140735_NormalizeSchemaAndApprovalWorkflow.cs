using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace DAL.EF.Migrations
{
    public partial class NormalizeSchemaAndApprovalWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_ContentLabels_ArticleId",
                table: "ContentLabels");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ArticleId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedDate",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AuthorId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsBreakingNews",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_PublishDate",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ConfPassword",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Articles",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Articles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Articles",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Articles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Articles",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArticleStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleStatuses", x => x.StatusId);
                });

            migrationBuilder.InsertData(
                table: "ArticleStatuses",
                columns: new[] { "StatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Draft" },
                    { 2, "Pending" },
                    { 3, "Published" },
                    { 4, "Rejected" }
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Roles] ON;
MERGE [Roles] AS target
USING (VALUES (1, N'Admin'), (2, N'Reader'), (3, N'Author')) AS source (Id, RoleName)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET RoleName = source.RoleName
WHEN NOT MATCHED THEN INSERT (Id, RoleName) VALUES (source.Id, source.RoleName);
SET IDENTITY_INSERT [Roles] OFF;");

            migrationBuilder.Sql("UPDATE [Articles] SET [StatusId] = 3, [CreatedAt] = [PublishDate];");

            migrationBuilder.CreateIndex(
                name: "UX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArticleId_CreatedDate",
                table: "Comments",
                columns: new[] { "ArticleId", "CreatedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorId_StatusId",
                table: "Articles",
                columns: new[] { "AuthorId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId_StatusId_PublishDate",
                table: "Articles",
                columns: new[] { "CategoryId", "StatusId", "PublishDate" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsBreakingNews_StatusId_PublishDate",
                table: "Articles",
                columns: new[] { "IsBreakingNews", "StatusId", "PublishDate" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ReviewedById",
                table: "Articles",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_StatusId_PublishDate",
                table: "Articles",
                columns: new[] { "StatusId", "PublishDate" },
                descending: new[] { false, true });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Articles_ViewsCount",
                table: "Articles",
                sql: "[ViewsCount] >= 0");

            migrationBuilder.CreateIndex(
                name: "UX_ArticleStatuses_Name",
                table: "ArticleStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_ArticleStatuses",
                table: "Articles",
                column: "StatusId",
                principalTable: "ArticleStatuses",
                principalColumn: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_ReviewedBy_Users",
                table: "Articles",
                column: "ReviewedById",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_ArticleStatuses",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Articles_ReviewedBy_Users",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "ArticleStatuses");

            migrationBuilder.DropIndex(
                name: "UX_Tags_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "UX_Roles_RoleName",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ArticleId_CreatedDate",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "UX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "UX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AuthorId_StatusId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CategoryId_StatusId_PublishDate",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsBreakingNews_StatusId_PublishDate",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ReviewedById",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_StatusId_PublishDate",
                table: "Articles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Articles_ViewsCount",
                table: "Articles");

            migrationBuilder.Sql("UPDATE [Users] SET [RoleId] = 2 WHERE [RoleId] = 3;");
            migrationBuilder.Sql("DELETE FROM [Roles] WHERE [Id] = 3;");
            migrationBuilder.Sql("UPDATE [Roles] SET [RoleName] = N'Member' WHERE [Id] = 2;");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Articles");

            migrationBuilder.AddColumn<string>(
                name: "ConfPassword",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ContentLabels_ArticleId",
                table: "ContentLabels",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArticleId",
                table: "Comments",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedDate",
                table: "Comments",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorId",
                table: "Articles",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsBreakingNews",
                table: "Articles",
                column: "IsBreakingNews");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishDate",
                table: "Articles",
                column: "PublishDate");
        }
    }
}
