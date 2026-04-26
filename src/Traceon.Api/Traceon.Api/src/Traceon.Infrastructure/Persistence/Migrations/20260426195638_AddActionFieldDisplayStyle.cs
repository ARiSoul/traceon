using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActionFieldDisplayStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayStyle",
                schema: "Traceon",
                table: "ActionFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayStyleConfigJson",
                schema: "Traceon",
                table: "ActionFields",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayStyle",
                schema: "Traceon",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "DisplayStyleConfigJson",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
