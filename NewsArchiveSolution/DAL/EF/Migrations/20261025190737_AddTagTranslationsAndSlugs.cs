using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations
{
    public partial class AddTagTranslationsAndSlugs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Tags",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tags",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Tags_NameBn",
                table: "Tags",
                column: "NameBn",
                unique: true,
                filter: "[NameBn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Tags_NameBn",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "UX_Tags_Slug",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tags");
        }
    }
}
