using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Manual.Tests.Migrations
{
    public partial class AddCorrelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "VersionedEventPayload",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "VersionedEventPayload");
        }
    }
}
