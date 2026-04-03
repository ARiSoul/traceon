using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackedActions_UserId_Name",
                schema: "Traceon",
                table: "TrackedActions");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                schema: "Traceon",
                table: "Tags");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "TrackedActions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "TrackedActions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "Tags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "Tags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "FieldDefinitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "FieldDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DataRetentionDays",
                schema: "Traceon",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 180);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionFields",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionEntryFields",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionEntryFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TrackedActions_UserId_Name",
                schema: "Traceon",
                table: "TrackedActions",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                schema: "Traceon",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackedActions_UserId_Name",
                schema: "Traceon",
                table: "TrackedActions");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                schema: "Traceon",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "TrackedActions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "TrackedActions");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "FieldDefinitions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "FieldDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "DataRetentionDays",
                schema: "Traceon",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionEntryFields");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionEntryFields");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "Traceon",
                table: "ActionEntries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Traceon",
                table: "ActionEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedActions_UserId_Name",
                schema: "Traceon",
                table: "TrackedActions",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                schema: "Traceon",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }
    }
}
