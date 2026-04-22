using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsRuleOffsetAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OffsetDirection",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OffsetTriggerValues",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "OffsetTriggerFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "OffsetValueFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "OffsetTriggerFieldId",
                principalSchema: "Traceon",
                principalTable: "ActionFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "OffsetValueFieldId",
                principalSchema: "Traceon",
                principalTable: "ActionFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropIndex(
                name: "IX_FieldAnalyticsRules_OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropIndex(
                name: "IX_FieldAnalyticsRules_OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "OffsetDirection",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "OffsetTriggerFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "OffsetTriggerValues",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "OffsetValueFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");
        }
    }
}
