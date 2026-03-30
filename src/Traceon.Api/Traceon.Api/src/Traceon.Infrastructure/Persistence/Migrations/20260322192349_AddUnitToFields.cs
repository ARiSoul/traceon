using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "Traceon",
                table: "FieldDefinitions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "UN");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "Traceon",
                table: "ActionFields",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "UN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "Traceon",
                table: "FieldDefinitions");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
