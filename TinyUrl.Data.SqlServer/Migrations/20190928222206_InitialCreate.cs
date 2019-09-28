using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TinyUrl.Data.SqlServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AliasKeys",
                columns: table => new
                {
                    Alias = table.Column<string>(type: "NVARCHAR(32) COLLATE SQL_Latin1_General_CP1_CS_AS", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasKeys", x => x.Alias);
                });

            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(maxLength: 32, nullable: false),
                    OriginalUrl = table.Column<string>(maxLength: 2048, nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Links_Alias",
                table: "Links",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Links_OriginalUrl",
                table: "Links",
                column: "OriginalUrl");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AliasKeys");

            migrationBuilder.DropTable(
                name: "Links");
        }
    }
}
