using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDependencyRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldDependencyRules",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TargetConstraint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldDependencyRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldDependencyRules_ActionFields_SourceFieldId",
                        column: x => x.SourceFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldDependencyRules_ActionFields_TargetFieldId",
                        column: x => x.TargetFieldId,
                        principalSchema: "Traceon",
                        principalTable: "ActionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldDependencyRules_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldDependencyRules_SourceFieldId",
                schema: "Traceon",
                table: "FieldDependencyRules",
                column: "SourceFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDependencyRules_TargetFieldId",
                schema: "Traceon",
                table: "FieldDependencyRules",
                column: "TargetFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDependencyRules_TrackedActionId",
                schema: "Traceon",
                table: "FieldDependencyRules",
                column: "TrackedActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldDependencyRules",
                schema: "Traceon");
        }
    }
}
