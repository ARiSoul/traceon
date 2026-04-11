using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "DiscountFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptImportConfigs_ActionFields_DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "DiscountFieldId",
                principalSchema: "Traceon",
                principalTable: "ActionFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptImportConfigs_ActionFields_DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptImportConfigs_DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");

            migrationBuilder.DropColumn(
                name: "DiscountFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs");
        }
    }
}
