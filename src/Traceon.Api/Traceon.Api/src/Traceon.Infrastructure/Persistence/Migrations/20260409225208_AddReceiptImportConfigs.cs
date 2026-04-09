using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptImportConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptImportConfigs",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DescriptionFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuantityFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitPriceFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptImportConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_ActionFields_DescriptionFieldId",
                        column: x => x.DescriptionFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_ActionFields_QuantityFieldId",
                        column: x => x.QuantityFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_ActionFields_ShopFieldId",
                        column: x => x.ShopFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_ActionFields_TotalFieldId",
                        column: x => x.TotalFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_ActionFields_UnitPriceFieldId",
                        column: x => x.UnitPriceFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptImportConfigs_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptMappingRules",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptImportConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptMappingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptMappingRules_ActionFields_TargetFieldId",
                        column: x => x.TargetFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptMappingRules_ReceiptImportConfigs_ReceiptImportConfigId",
                        column: x => x.ReceiptImportConfigId,
                        principalSchema: "Traceon",
                        principalTable: "ReceiptImportConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_DescriptionFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "DescriptionFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_QuantityFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "QuantityFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_ShopFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "ShopFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_TotalFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "TotalFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_TrackedActionId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "TrackedActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptImportConfigs_UnitPriceFieldId",
                schema: "Traceon",
                table: "ReceiptImportConfigs",
                column: "UnitPriceFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptMappingRules_ReceiptImportConfigId",
                schema: "Traceon",
                table: "ReceiptMappingRules",
                column: "ReceiptImportConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptMappingRules_TargetFieldId",
                schema: "Traceon",
                table: "ReceiptMappingRules",
                column: "TargetFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptMappingRules",
                schema: "Traceon");

            migrationBuilder.DropTable(
                name: "ReceiptImportConfigs",
                schema: "Traceon");
        }
    }
}
