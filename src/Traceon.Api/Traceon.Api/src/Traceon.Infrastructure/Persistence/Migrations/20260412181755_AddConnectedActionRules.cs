using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectedActionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectedActionRules",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConditionsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FieldMappingsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CopyNotes = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CopyDate = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectedActionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectedActionRules_TrackedActions_SourceTrackedActionId",
                        column: x => x.SourceTrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectedActionRules_TrackedActions_TargetTrackedActionId",
                        column: x => x.TargetTrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectedActionRules_SourceTrackedActionId",
                schema: "Traceon",
                table: "ConnectedActionRules",
                column: "SourceTrackedActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectedActionRules_TargetTrackedActionId",
                schema: "Traceon",
                table: "ConnectedActionRules",
                column: "TargetTrackedActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectedActionRules",
                schema: "Traceon");
        }
    }
}
