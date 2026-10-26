using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace DAL.EF.Migrations
{
    public partial class AddCommentModeration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "Comments",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeratedById",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportReasons",
                columns: table => new
                {
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportReasons", x => x.ReasonId);
                });

            migrationBuilder.CreateTable(
                name: "CommentReports",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReports", x => new { x.CommentId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CommentReports_Comments",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReports_ReportReasons",
                        column: x => x.ReasonId,
                        principalTable: "ReportReasons",
                        principalColumn: "ReasonId");
                    table.ForeignKey(
                        name: "FK_CommentReports_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.InsertData(
                table: "ReportReasons",
                columns: new[] { "ReasonId", "Name" },
                values: new object[,]
                {
                    { 1, "Spam" },
                    { 2, "Abuse or harassment" },
                    { 3, "Off topic" },
                    { 4, "Misinformation" },
                    { 5, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArticleId_IsHidden",
                table: "Comments",
                columns: new[] { "ArticleId", "IsHidden" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ModeratedById",
                table: "Comments",
                column: "ModeratedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_ReasonId",
                table: "CommentReports",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_UserId",
                table: "CommentReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_ReportReasons_Name",
                table: "ReportReasons",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ModeratedBy_Users",
                table: "Comments",
                column: "ModeratedById",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ModeratedBy_Users",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "CommentReports");

            migrationBuilder.DropTable(
                name: "ReportReasons");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ArticleId_IsHidden",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ModeratedById",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ModeratedById",
                table: "Comments");
        }
    }
}
