using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyEntryFieldValueColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                schema: "Traceon",
                table: "EntryTemplateFields");

            migrationBuilder.DropColumn(
                name: "Value",
                schema: "Traceon",
                table: "ActionEntryFields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "Traceon",
                table: "EntryTemplateFields",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "Traceon",
                table: "ActionEntryFields",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
