using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDropdownTrendColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DropdownTrendAggregation",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DropdownTrendChartType",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DropdownTrendValueFieldId",
                schema: "Traceon",
                table: "ActionFields",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropdownTrendAggregation",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "DropdownTrendChartType",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "DropdownTrendValueFieldId",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
