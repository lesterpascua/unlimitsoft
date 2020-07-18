using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.CQRS.Test.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 60, nullable: false),
                    LastName = table.Column<string>(maxLength: 120, nullable: false),
                    CID = table.Column<string>(maxLength: 32, nullable: false),
                    CustomerType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VersionedEventPayload",
                columns: table => new
                {
                    SourceID = table.Column<Guid>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    EntityID = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    EventType = table.Column<string>(maxLength: 256, nullable: false),
                    IsPubliched = table.Column<bool>(nullable: false),
                    IsDomainEvent = table.Column<bool>(nullable: false),
                    Action = table.Column<string>(nullable: true),
                    ActionType = table.Column<string>(nullable: true),
                    PrevSnapshot = table.Column<string>(nullable: true),
                    CurrSnapshot = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionedEventPayload", x => new { x.SourceID, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CustomerID = table.Column<Guid>(nullable: false),
                    Street = table.Column<string>(maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Address_Customer_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customer",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CustomerID",
                table: "Address",
                column: "CustomerID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "VersionedEventPayload");

            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
