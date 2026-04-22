using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptScopedDiscountSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptDiscountTypeValue",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptImportBatchId",
                schema: "Traceon",
                table: "ActionEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "ReceiptDiscountTypeFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntries_ReceiptImportBatchId",
                schema: "Traceon",
                table: "ActionEntries",
                column: "ReceiptImportBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptImportConfigs_ActionFields_ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "ReceiptDiscountTypeFieldId",
                principalSchema: "Traceon",
                principalTable: "ActionFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptImportConfigs_ActionFields_ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptImportConfigs_ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ActionEntries_ReceiptImportBatchId",
                schema: "Traceon",
                table: "ActionEntries");

            migrationBuilder.DropColumn(
                name: "ReceiptDiscountTypeFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropColumn(
                name: "ReceiptDiscountTypeValue",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropColumn(
                name: "ReceiptImportBatchId",
                schema: "Traceon",
                table: "ActionEntries");
        }
    }
}
