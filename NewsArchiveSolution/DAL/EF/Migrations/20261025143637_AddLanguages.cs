using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace DAL.EF.Migrations
{
    public partial class AddLanguages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionBn",
                table: "Categories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Articles",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageCode);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "LanguageCode", "Name", "NativeName" },
                values: new object[,]
                {
                    { "bn", "Bangla", "বাংলা" },
                    { "en", "English", "English" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Categories_NameBn",
                table: "Categories",
                column: "NameBn",
                unique: true,
                filter: "[NameBn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_LanguageCode_StatusId_PublishDate",
                table: "Articles",
                columns: new[] { "LanguageCode", "StatusId", "PublishDate" },
                descending: new[] { false, false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Languages",
                table: "Articles",
                column: "LanguageCode",
                principalTable: "Languages",
                principalColumn: "LanguageCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Languages",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropIndex(
                name: "UX_Categories_NameBn",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_LanguageCode_StatusId_PublishDate",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "DescriptionBn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Articles");
        }
    }
}
