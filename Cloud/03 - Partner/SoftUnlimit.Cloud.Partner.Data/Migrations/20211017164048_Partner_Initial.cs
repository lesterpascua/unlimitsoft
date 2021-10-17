using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoftUnlimit.Cloud.Partner.Data.Migrations
{
    public partial class Partner_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "partner");

            migrationBuilder.CreateTable(
                name: "JnRewardComplete",
                schema: "partner",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identifier of the event"),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "Primary key for the entity is unique for all the system."),
                    Version = table.Column<long>(type: "bigint", nullable: false, comment: "Event version number alwais is incremental for the same SourceId."),
                    ServiceId = table.Column<int>(type: "int", nullable: false, comment: "Service identifier."),
                    WorkerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Worker identifier where the event was generate."),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Correlation of the event, indicate the trace were the event was generate."),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event create date."),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Name of the event."),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Body of the event serialized as json."),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identity owner of the event."),
                    PartnerId = table.Column<int>(type: "int", nullable: true, comment: "Partner identifier where the event comming from (if null is internal system)."),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Retry attempt for this event."),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date where the event was process complete.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JnRewardComplete", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JnRewardPending",
                schema: "partner",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identifier of the event"),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "Primary key for the entity is unique for all the system."),
                    Version = table.Column<long>(type: "bigint", nullable: false, comment: "Event version number alwais is incremental for the same SourceId."),
                    ServiceId = table.Column<int>(type: "int", nullable: false, comment: "Service identifier."),
                    WorkerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Worker identifier where the event was generate."),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Correlation of the event, indicate the trace were the event was generate."),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event create date."),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Name of the event."),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Body of the event serialized as json."),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identity owner of the event."),
                    PartnerId = table.Column<int>(type: "int", nullable: true, comment: "Partner identifier where the event comming from (if null is internal system)."),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Retry attempt for this event."),
                    Scheduler = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Scheduler time popone this event.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JnRewardPending", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleforceComplete",
                schema: "partner",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identifier of the event"),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "Primary key for the entity is unique for all the system."),
                    Version = table.Column<long>(type: "bigint", nullable: false, comment: "Event version number alwais is incremental for the same SourceId."),
                    ServiceId = table.Column<int>(type: "int", nullable: false, comment: "Service identifier."),
                    WorkerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Worker identifier where the event was generate."),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Correlation of the event, indicate the trace were the event was generate."),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event create date."),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Name of the event."),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Body of the event serialized as json."),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identity owner of the event."),
                    PartnerId = table.Column<int>(type: "int", nullable: true, comment: "Partner identifier where the event comming from (if null is internal system)."),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Retry attempt for this event."),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date where the event was process complete.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleforceComplete", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleforcePending",
                schema: "partner",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identifier of the event"),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "Primary key for the entity is unique for all the system."),
                    Version = table.Column<long>(type: "bigint", nullable: false, comment: "Event version number alwais is incremental for the same SourceId."),
                    ServiceId = table.Column<int>(type: "int", nullable: false, comment: "Service identifier."),
                    WorkerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Worker identifier where the event was generate."),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Correlation of the event, indicate the trace were the event was generate."),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event create date."),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Name of the event."),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Body of the event serialized as json."),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identity owner of the event."),
                    PartnerId = table.Column<int>(type: "int", nullable: true, comment: "Partner identifier where the event comming from (if null is internal system)."),
                    Retry = table.Column<int>(type: "int", nullable: false, comment: "Retry attempt for this event."),
                    Scheduler = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Scheduler time popone this event.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleforcePending", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardComplete_CorrelationId",
                schema: "partner",
                table: "JnRewardComplete",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardComplete_Created",
                schema: "partner",
                table: "JnRewardComplete",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardComplete_EventId",
                schema: "partner",
                table: "JnRewardComplete",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardComplete_Name",
                schema: "partner",
                table: "JnRewardComplete",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardComplete_SourceId",
                schema: "partner",
                table: "JnRewardComplete",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardPending_CorrelationId",
                schema: "partner",
                table: "JnRewardPending",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardPending_Created",
                schema: "partner",
                table: "JnRewardPending",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardPending_EventId",
                schema: "partner",
                table: "JnRewardPending",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardPending_Name",
                schema: "partner",
                table: "JnRewardPending",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JnRewardPending_SourceId",
                schema: "partner",
                table: "JnRewardPending",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforceComplete_CorrelationId",
                schema: "partner",
                table: "SaleforceComplete",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforceComplete_Created",
                schema: "partner",
                table: "SaleforceComplete",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforceComplete_EventId",
                schema: "partner",
                table: "SaleforceComplete",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforceComplete_Name",
                schema: "partner",
                table: "SaleforceComplete",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforceComplete_SourceId",
                schema: "partner",
                table: "SaleforceComplete",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforcePending_CorrelationId",
                schema: "partner",
                table: "SaleforcePending",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforcePending_Created",
                schema: "partner",
                table: "SaleforcePending",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforcePending_EventId",
                schema: "partner",
                table: "SaleforcePending",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforcePending_Name",
                schema: "partner",
                table: "SaleforcePending",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SaleforcePending_SourceId",
                schema: "partner",
                table: "SaleforcePending",
                column: "SourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JnRewardComplete",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "JnRewardPending",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "SaleforceComplete",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "SaleforcePending",
                schema: "partner");
        }
    }
}
