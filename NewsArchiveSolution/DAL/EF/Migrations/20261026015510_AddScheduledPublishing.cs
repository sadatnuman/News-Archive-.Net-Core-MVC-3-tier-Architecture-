using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations
{
    public partial class AddScheduledPublishing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "Articles",
                type: "datetime",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ArticleStatuses",
                columns: new[] { "StatusId", "Name" },
                values: new object[] { 5, "Scheduled" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_StatusId_ScheduledAt",
                table: "Articles",
                columns: new[] { "StatusId", "ScheduledAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Articles_StatusId_ScheduledAt",
                table: "Articles");

            migrationBuilder.DeleteData(
                table: "ArticleStatuses",
                keyColumn: "StatusId",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "Articles");
        }
    }
}
