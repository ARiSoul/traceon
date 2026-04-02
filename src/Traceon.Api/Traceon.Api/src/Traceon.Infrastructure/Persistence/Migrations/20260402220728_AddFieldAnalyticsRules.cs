using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldAnalyticsRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldAnalyticsRules",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasureFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupByFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilterFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FilterValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Aggregation = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DisplayType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldAnalyticsRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldAnalyticsRules_ActionFields_FilterFieldId",
                        column: x => x.FilterFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldAnalyticsRules_ActionFields_GroupByFieldId",
                        column: x => x.GroupByFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldAnalyticsRules_ActionFields_MeasureFieldId",
                        column: x => x.MeasureFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldAnalyticsRules_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_FilterFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "FilterFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_GroupByFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "GroupByFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_MeasureFieldId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "MeasureFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldAnalyticsRules_TrackedActionId",
                schema: "Traceon",
                table: "FieldAnalyticsRules",
                column: "TrackedActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldAnalyticsRules",
                schema: "Traceon");
        }
    }
}
