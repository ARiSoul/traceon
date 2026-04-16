using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActionChartVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionChartVisibilities",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackedActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiddenKeys = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionChartVisibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionChartVisibilities_TrackedActions_TrackedActionId",
                        column: x => x.TrackedActionId,
                        principalSchema: "Traceon",
                        principalTable: "TrackedActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionChartVisibilities_TrackedActionId",
                schema: "Traceon",
                table: "ActionChartVisibilities",
                column: "TrackedActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionChartVisibilities_UserId_TrackedActionId",
                schema: "Traceon",
                table: "ActionChartVisibilities",
                columns: new[] { "UserId", "TrackedActionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionChartVisibilities",
                schema: "Traceon");
        }
    }
}
