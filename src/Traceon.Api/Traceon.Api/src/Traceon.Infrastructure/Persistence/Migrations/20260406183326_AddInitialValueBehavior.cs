using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialValueBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialValueBehavior",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialValuePeriodCount",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialValuePeriodUnit",
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
                name: "InitialValueBehavior",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "InitialValuePeriodCount",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "InitialValuePeriodUnit",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
