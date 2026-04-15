using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDropdownValueMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DropdownValueMetadataFields",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DropdownValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropdownValueMetadataFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DropdownValueMetadataFields_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalSchema: "Traceon",
                        principalTable: "FieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DropdownValueMetadataValues",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DropdownValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetadataFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropdownValueMetadataValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DropdownValueMetadataValues_DropdownValueMetadataFields_MetadataFieldId",
                        column: x => x.MetadataFieldId,
                        principalSchema: "Traceon",
                        principalTable: "DropdownValueMetadataFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropdownValueMetadataValues_DropdownValues_DropdownValueId",
                        column: x => x.DropdownValueId,
                        principalSchema: "Traceon",
                        principalTable: "DropdownValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "FilterMetadataFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "GroupByMetadataFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValueMetadataFields_FieldDefinitionId",
                schema: "Traceon",
                table: "DropdownValueMetadataFields",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValueMetadataFields_FieldDefinitionId_Name",
                schema: "Traceon",
                table: "DropdownValueMetadataFields",
                columns: new[] { "FieldDefinitionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValueMetadataValues_DropdownValueId",
                schema: "Traceon",
                table: "DropdownValueMetadataValues",
                column: "DropdownValueId");

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValueMetadataValues_DropdownValueId_MetadataFieldId",
                schema: "Traceon",
                table: "DropdownValueMetadataValues",
                columns: new[] { "DropdownValueId", "MetadataFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValueMetadataValues_MetadataFieldId",
                schema: "Traceon",
                table: "DropdownValueMetadataValues",
                column: "MetadataFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAnalyticsRules_DropdownValueMetadataFields_FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "FilterMetadataFieldId",
                principalSchema: "Traceon",
                principalTable: "DropdownValueMetadataFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAnalyticsRules_DropdownValueMetadataFields_GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "GroupByMetadataFieldId",
                principalSchema: "Traceon",
                principalTable: "DropdownValueMetadataFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAnalyticsRules_DropdownValueMetadataFields_FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropForeignKey(
                name: "FK_FieldAnalyticsRules_DropdownValueMetadataFields_GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropTable(
                name: "DropdownValueMetadataValues",
                schema: "Traceon");

            migrationBuilder.DropTable(
                name: "DropdownValueMetadataFields",
                schema: "Traceon");

            migrationBuilder.DropIndex(
                name: "IX_FieldAnalyticsRules_FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropIndex(
                name: "IX_FieldAnalyticsRules_GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "FilterMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");

            migrationBuilder.DropColumn(
                name: "GroupByMetadataFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules");
        }
    }
}
