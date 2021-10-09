using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.WebApi.Sources.Data.Migrations
{
    public partial class AddLockTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lock",
                schema: "dbo",
                columns: table => new
                {
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lock",
                schema: "dbo");
        }
    }
}
