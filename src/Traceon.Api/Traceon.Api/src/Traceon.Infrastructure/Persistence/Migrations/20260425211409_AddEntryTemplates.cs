using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntryTemplates",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryTemplates_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryTemplateFields",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTemplateFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryTemplateFields_ActionFields_ActionFieldId",
                        column: x => x.ActionFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryTemplateFields_EntryTemplates_EntryTemplateId",
                        column: x => x.EntryTemplateId,
                        principalSchema: "Traceon",
                        principalTable: "EntryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryTemplateFields_ActionFieldId",
                schema: "Traceon",
                table: "EntryTemplateFields",
                column: "ActionFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTemplateFields_EntryTemplateId",
                schema: "Traceon",
                table: "EntryTemplateFields",
                column: "EntryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTemplates_TrackedActionId",
                schema: "Traceon",
                table: "EntryTemplates",
                column: "TrackedActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryTemplateFields",
                schema: "Traceon");

            migrationBuilder.DropTable(
                name: "EntryTemplates",
                schema: "Traceon");
        }
    }
}
