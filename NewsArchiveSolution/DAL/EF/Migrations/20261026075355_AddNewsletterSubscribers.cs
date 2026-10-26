using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations
{
    public partial class AddNewsletterSubscribers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    SubscriberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    ConfirmTokenHash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    UnsubscribeToken = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UnsubscribedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastSentAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.SubscriberId);
                    table.ForeignKey(
                        name: "FK_Subscribers_Languages",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "LanguageCode");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Language_Status",
                table: "Subscribers",
                columns: new[] { "LanguageCode", "ConfirmedAt", "UnsubscribedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Subscribers_ConfirmTokenHash",
                table: "Subscribers",
                column: "ConfirmTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Subscribers_Email",
                table: "Subscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Subscribers_UnsubscribeToken",
                table: "Subscribers",
                column: "UnsubscribeToken",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscribers");
        }
    }
}
