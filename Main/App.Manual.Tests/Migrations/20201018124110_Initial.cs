using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Manual.Tests.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dummy",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dummy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersionedEventPayload",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CommandId = table.Column<Guid>(nullable: false),
                    CommandType = table.Column<string>(maxLength: 255, nullable: false),
                    SourceId = table.Column<string>(maxLength: 36, nullable: false),
                    EntityType = table.Column<string>(maxLength: 255, nullable: false),
                    ServiceId = table.Column<long>(nullable: false),
                    WorkerId = table.Column<string>(nullable: false),
                    EventName = table.Column<string>(maxLength: 255, nullable: false),
                    IsStartAction = table.Column<bool>(nullable: false),
                    IsFinalAction = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    IsPubliched = table.Column<bool>(nullable: false),
                    IsDomain = table.Column<bool>(nullable: false),
                    BodyType = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: false),
                    Version = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionedEventPayload", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VersionedEventPayload_SourceId_Version",
                table: "VersionedEventPayload",
                columns: new[] { "SourceId", "Version" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dummy");

            migrationBuilder.DropTable(
                name: "VersionedEventPayload");
        }
    }
}
