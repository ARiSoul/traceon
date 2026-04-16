using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomCharts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomCharts",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MeasureFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Aggregation = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GroupByFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimeGrouping = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ChartType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ColorPalette = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilterConditionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SortDescending = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxGroups = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomCharts_ActionFields_GroupByFieldId",
                        column: x => x.GroupByFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomCharts_ActionFields_MeasureFieldId",
                        column: x => x.MeasureFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomCharts_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomCharts_GroupByFieldId",
                schema: "Traceon",
                table: "CustomCharts",
                column: "GroupByFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomCharts_MeasureFieldId",
                schema: "Traceon",
                table: "CustomCharts",
                column: "MeasureFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomCharts_TrackedActionId",
                schema: "Traceon",
                table: "CustomCharts",
                column: "TrackedActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomCharts",
                schema: "Traceon");
        }
    }
}
