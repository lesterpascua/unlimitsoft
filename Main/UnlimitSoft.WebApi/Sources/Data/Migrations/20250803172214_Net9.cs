using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnlimitSoft.WebApi.Sources.Data.Migrations
{
    /// <inheritdoc />
    public partial class Net9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventName",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "Payload",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.AlterTable(
                name: "VersionedEvent",
                schema: "dbo",
                comment: "Store the event generated for the service. All event will be taken from here and publish");

            migrationBuilder.AlterColumn<long>(
                name: "Version",
                schema: "dbo",
                table: "VersionedEvent",
                type: "bigint",
                nullable: false,
                comment: "Version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y \r\n    /// es el que ella poseia en el instante en que fue generado el evento",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "uniqueidentifier",
                nullable: false,
                comment: "PKey of the identity where event was attached",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Scheduled",
                schema: "dbo",
                table: "VersionedEvent",
                type: "datetime2",
                nullable: true,
                comment: "Date when the event was scheduled to publish",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPubliched",
                schema: "dbo",
                table: "VersionedEvent",
                type: "bit",
                nullable: false,
                comment: "Indicate if the event was already published",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                schema: "dbo",
                table: "VersionedEvent",
                type: "datetime2",
                nullable: false,
                comment: "Date when the event was created",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                comment: "Operation correlation identifier",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "dbo",
                table: "VersionedEvent",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Event unique identifier",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Json with the body serialized");

            migrationBuilder.AddColumn<bool>(
                name: "IsDomainEvent",
                schema: "dbo",
                table: "VersionedEvent",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Specify if an event belown to domain. This have optimization propouse");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                comment: "Name of the event. This is use to identified the event type");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Identifier of the service where the event below");

            migrationBuilder.AddColumn<string>(
                name: "WorkerId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Identifier of the worker were the event is create");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionedEvent_Created",
                schema: "dbo",
                table: "VersionedEvent",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_VersionedEvent_IsPubliched",
                schema: "dbo",
                table: "VersionedEvent",
                column: "IsPubliched");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VersionedEvent_Created",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropIndex(
                name: "IX_VersionedEvent_IsPubliched",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "Body",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "IsDomainEvent",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.DropColumn(
                name: "WorkerId",
                schema: "dbo",
                table: "VersionedEvent");

            migrationBuilder.AlterTable(
                name: "VersionedEvent",
                schema: "dbo",
                oldComment: "Store the event generated for the service. All event will be taken from here and publish");

            migrationBuilder.AlterColumn<long>(
                name: "Version",
                schema: "dbo",
                table: "VersionedEvent",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y \r\n    /// es el que ella poseia en el instante en que fue generado el evento");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "PKey of the identity where event was attached");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Scheduled",
                schema: "dbo",
                table: "VersionedEvent",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Date when the event was scheduled to publish");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPubliched",
                schema: "dbo",
                table: "VersionedEvent",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicate if the event was already published");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                schema: "dbo",
                table: "VersionedEvent",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Date when the event was created");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true,
                oldComment: "Operation correlation identifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "dbo",
                table: "VersionedEvent",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Event unique identifier");

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                schema: "dbo",
                table: "VersionedEvent",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
