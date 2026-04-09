using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_ExpiresAtUtc",
                schema: "Traceon",
                table: "UserRefreshTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_TokenHash",
                schema: "Traceon",
                table: "UserRefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId",
                schema: "Traceon",
                table: "UserRefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRefreshTokens",
                schema: "Traceon");
        }
    }
}
