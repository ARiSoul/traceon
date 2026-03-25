using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricsColumnsToActionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SummaryMetrics",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 63);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetValue",
                schema: "Traceon",
                table: "ActionFields",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrendAggregation",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrendChartType",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryMetrics",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "TrendAggregation",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "TrendChartType",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
