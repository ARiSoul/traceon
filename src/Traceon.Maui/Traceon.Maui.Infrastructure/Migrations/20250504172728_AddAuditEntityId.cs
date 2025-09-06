using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arisoul.Traceon.Maui.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "Audits",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Audits");
        }
    }
}
