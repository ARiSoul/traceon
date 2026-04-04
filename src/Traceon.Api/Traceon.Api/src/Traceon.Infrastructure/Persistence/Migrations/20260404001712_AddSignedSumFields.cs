using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSignedSumFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NegativeValues",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "SignFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "SignFieldId",
                principalSchema: "Traceon",
                principalTable: "ActionFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAnalyticsRules_ActionFields_SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropIndex(
                name: "IX_FieldAnalyticsRules_SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "NegativeValues",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "SignFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");
        }
    }
}
