using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arisoul.Traceon.Maui.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActionEntryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionFields",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "FieldValues",
                table: "ActionEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ActionFields",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionFields",
                table: "ActionFields",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ActionEntryFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionFieldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionEntryFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionEntryFields_ActionEntries_ActionEntryId",
                        column: x => x.ActionEntryId,
                        principalTable: "ActionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionEntryFields_ActionFields_ActionFieldId",
                        column: x => x.ActionFieldId,
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionEntryFields_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionFields_ActionId",
                table: "ActionFields",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntryFields_ActionEntryId",
                table: "ActionEntryFields",
                column: "ActionEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntryFields_ActionFieldId",
                table: "ActionEntryFields",
                column: "ActionFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntryFields_FieldDefinitionId",
                table: "ActionEntryFields",
                column: "FieldDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionEntryFields");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionFields",
                table: "ActionFields");

            migrationBuilder.DropIndex(
                name: "IX_ActionFields_ActionId",
                table: "ActionFields");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ActionFields");

            migrationBuilder.AddColumn<string>(
                name: "FieldValues",
                table: "ActionEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionFields",
                table: "ActionFields",
                columns: new[] { "ActionId", "FieldDefinitionId" });
        }
    }
}
