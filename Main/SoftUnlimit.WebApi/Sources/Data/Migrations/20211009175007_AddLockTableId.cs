using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.WebApi.Sources.Data.Migrations
{
    public partial class AddLockTableId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "dbo",
                table: "Lock",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lock",
                schema: "dbo",
                table: "Lock",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lock",
                schema: "dbo",
                table: "Lock");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "dbo",
                table: "Lock");
        }
    }
}
