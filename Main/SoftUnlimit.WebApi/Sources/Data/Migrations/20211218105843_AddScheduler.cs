using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.WebApi.Sources.Data.Migrations
{
    public partial class AddScheduler : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Scheduled",
                schema: "dbo",
                table: "VersionedEvent",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scheduled",
                schema: "dbo",
                table: "VersionedEvent");
        }
    }
}
