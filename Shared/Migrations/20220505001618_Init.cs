using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    LastUpdateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    LastUpdateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdLinks",
                columns: table => new
                {
                    Link = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    SearchLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    LastUpdateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdLinks", x => x.Link);
                    table.ForeignKey(
                        name: "FK_AdLinks_SearchLinks_SearchLinkId",
                        column: x => x.SearchLinkId,
                        principalTable: "SearchLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchLinkUser",
                columns: table => new
                {
                    SearchLinksId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLinkUser", x => new { x.SearchLinksId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_SearchLinkUser_SearchLinks_SearchLinksId",
                        column: x => x.SearchLinksId,
                        principalTable: "SearchLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLinkUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdLinks_SearchLinkId",
                table: "AdLinks",
                column: "SearchLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLinkUser_UsersId",
                table: "SearchLinkUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdLinks");

            migrationBuilder.DropTable(
                name: "SearchLinkUser");

            migrationBuilder.DropTable(
                name: "SearchLinks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
