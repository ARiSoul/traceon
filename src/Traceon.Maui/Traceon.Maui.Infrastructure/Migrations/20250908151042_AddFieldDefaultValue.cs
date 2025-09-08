using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arisoul.Traceon.Maui.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "FieldDefinitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "ActionFields",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "FieldDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "ActionFields");
        }
    }
}
