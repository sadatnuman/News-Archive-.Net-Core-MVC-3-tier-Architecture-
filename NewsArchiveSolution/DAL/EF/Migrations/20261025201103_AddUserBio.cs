using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations
{
    public partial class AddUserBio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");
        }
    }
}
