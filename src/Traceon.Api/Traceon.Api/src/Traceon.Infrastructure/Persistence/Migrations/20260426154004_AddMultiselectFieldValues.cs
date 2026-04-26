using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiselectFieldValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultiselect",
                schema: "Traceon",
                table: "ActionFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ActionEntryFieldValues",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionEntryFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionEntryFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionEntryFieldValues_ActionEntryFields_ActionEntryFieldId",
                        column: x => x.ActionEntryFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionEntryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryTemplateFieldValues",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryTemplateFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTemplateFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryTemplateFieldValues_EntryTemplateFields_EntryTemplateFieldId",
                        column: x => x.EntryTemplateFieldId,
                        principalSchema: "Traceon",
                        principalTable: "EntryTemplateFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntryFieldValues_ActionEntryFieldId",
                schema: "Traceon",
                table: "ActionEntryFieldValues",
                column: "ActionEntryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionEntryFieldValues_Value",
                schema: "Traceon",
                table: "ActionEntryFieldValues",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTemplateFieldValues_EntryTemplateFieldId",
                schema: "Traceon",
                table: "EntryTemplateFieldValues",
                column: "EntryTemplateFieldId");

            migrationBuilder.Sql(@"
INSERT INTO [Traceon].[ActionEntryFieldValues]
    ([Id], [ActionEntryFieldId], [Value], [Order], [CreatedAtUtc], [IsDeleted])
SELECT NEWID(), [Id], [Value], 0, GETUTCDATE(), 0
FROM [Traceon].[ActionEntryFields]
WHERE [Value] IS NOT NULL AND LTRIM(RTRIM([Value])) <> '';

INSERT INTO [Traceon].[EntryTemplateFieldValues]
    ([Id], [EntryTemplateFieldId], [Value], [Order], [CreatedAtUtc], [IsDeleted])
SELECT NEWID(), [Id], [Value], 0, GETUTCDATE(), 0
FROM [Traceon].[EntryTemplateFields]
WHERE [Value] IS NOT NULL AND LTRIM(RTRIM([Value])) <> '';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionEntryFieldValues",
                schema: "Traceon");

            migrationBuilder.DropTable(
                name: "EntryTemplateFieldValues",
                schema: "Traceon");

            migrationBuilder.DropColumn(
                name: "IsMultiselect",
                schema: "Traceon",
                table: "ActionFields");
        }
    }
}
