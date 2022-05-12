using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    public partial class restoreAdLinkProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "AdLinks",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks",
                column: "Link");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "AdLinks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdLinks",
                table: "AdLinks",
                column: "Id");
        }
    }
}
