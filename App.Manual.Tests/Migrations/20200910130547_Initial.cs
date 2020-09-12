using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Manual.Tests.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinaryVersionedEventPayload",
                columns: table => new
                {
                    SourceID = table.Column<string>(maxLength: 36, nullable: false),
                    Version = table.Column<long>(nullable: false),
                    CreatorID = table.Column<string>(maxLength: 36, nullable: false),
                    CreatorName = table.Column<string>(maxLength: 255, nullable: false),
                    ServiceID = table.Column<long>(nullable: false),
                    WorkerID = table.Column<int>(nullable: false),
                    EventName = table.Column<string>(maxLength: 255, nullable: false),
                    IsStartAction = table.Column<bool>(nullable: false),
                    IsFinalAction = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    IsPubliched = table.Column<bool>(nullable: false),
                    IsDomain = table.Column<bool>(nullable: false),
                    RawData = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinaryVersionedEventPayload", x => new { x.SourceID, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "Dummy",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dummy", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BinaryVersionedEventPayload_CreatorName",
                table: "BinaryVersionedEventPayload",
                column: "CreatorName");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryVersionedEventPayload_EventName",
                table: "BinaryVersionedEventPayload",
                column: "EventName");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryVersionedEventPayload_SourceID",
                table: "BinaryVersionedEventPayload",
                column: "SourceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinaryVersionedEventPayload");

            migrationBuilder.DropTable(
                name: "Dummy");
        }
    }
}
