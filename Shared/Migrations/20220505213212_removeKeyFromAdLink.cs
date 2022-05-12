using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    public partial class removeKeyFromAdLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks",
                column: "Link");
        }
    }
}
