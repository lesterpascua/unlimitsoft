using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.Cloud.VirusScan.Data.Migrations
{
    public partial class VirusScan_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "viruscan");

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "viruscan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VirusDetected = table.Column<int>(type: "int", nullable: false, comment: "Amount of request with virus detected from the FirstVirusDetected date"),
                    FirstVirusDetected = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Date when some request has mark with virus for first time"),
                    HistoryVirusDetected = table.Column<int>(type: "int", nullable: false, comment: "Amount of virus detected for this user in the entirely history"),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersionedEvent",
                schema: "viruscan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPubliched = table.Column<bool>(type: "bit", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionedEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Complete",
                schema: "viruscan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "User owner of the file. Null if no user asociate"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identifier of the request"),
                    CorrelationId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true, comment: "CorrelationId asociate to the process. Unique value to identifier the source of the operation"),
                    BlobUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "Unique BlobUri identifier of the file"),
                    HasVirus = table.Column<bool>(type: "bit", nullable: false, comment: "Indicate if the file has virus or not"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date where request is created"),
                    Scanned = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date when the file was scanned"),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Number of retry attemp for the file"),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complete", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complete_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "viruscan",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pending",
                schema: "viruscan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "User owner of the file. Null if no user asociate"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identifier of the request"),
                    CorrelationId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true, comment: "CorrelationId asociate to the process. Unique value to identifier the source of the operation"),
                    BlobUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "Unique BlobUri identifier of the file"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "Status of the request. (1 - Pending, 2 - Approved, 3 - Error)"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date where request is created"),
                    Scanned = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Date when the file was scanned"),
                    Scheduler = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date where the file will be scanned"),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Number of retry attemp for the file"),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Metadata asociate to the file, serialize in json"),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pending", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pending_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "viruscan",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complete_CorrelationId",
                schema: "viruscan",
                table: "Complete",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Complete_CustomerId",
                schema: "viruscan",
                table: "Complete",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Complete_RequestId",
                schema: "viruscan",
                table: "Complete",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Pending_CorrelationId",
                schema: "viruscan",
                table: "Pending",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Pending_CustomerId",
                schema: "viruscan",
                table: "Pending",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pending_RequestId",
                schema: "viruscan",
                table: "Pending",
                column: "RequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Complete",
                schema: "viruscan");

            migrationBuilder.DropTable(
                name: "Pending",
                schema: "viruscan");

            migrationBuilder.DropTable(
                name: "VersionedEvent",
                schema: "viruscan");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "viruscan");
        }
    }
}
