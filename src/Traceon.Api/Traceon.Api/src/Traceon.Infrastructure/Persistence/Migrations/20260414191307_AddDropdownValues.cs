using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDropdownValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DropdownValues",
                schema: "Traceon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropdownValues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValues_FieldDefinitionId",
                schema: "Traceon",
                table: "DropdownValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DropdownValues_FieldDefinitionId_Value",
                schema: "Traceon",
                table: "DropdownValues",
                columns: new[] { "FieldDefinitionId", "Value" },
                unique: true);

            // Seed: populate DropdownValues from existing FieldDefinitions.DropdownValues pipe-delimited strings.
            // Skips composite dropdowns (ref: prefix) and empty/null values.
            migrationBuilder.Sql("""
                ;WITH FieldsWithValues AS (
                    SELECT [Id] AS FieldDefinitionId, [DropdownValues]
                    FROM [Traceon].[FieldDefinitions]
                    WHERE [DropdownValues] IS NOT NULL
                      AND [DropdownValues] <> ''
                      AND [DropdownValues] NOT LIKE 'ref:%'
                      AND [IsDeleted] = 0
                ),
                SplitValues AS (
                    SELECT
                        f.FieldDefinitionId,
                        LTRIM(RTRIM(s.[value])) AS [Value],
                        ROW_NUMBER() OVER (PARTITION BY f.FieldDefinitionId ORDER BY (SELECT NULL)) - 1 AS SortOrder
                    FROM FieldsWithValues f
                    CROSS APPLY STRING_SPLIT(f.[DropdownValues], '|') s
                    WHERE LTRIM(RTRIM(s.[value])) <> ''
                )
                INSERT INTO [Traceon].[DropdownValues]
                    ([Id], [FieldDefinitionId], [Value], [SortOrder], [CreatedAtUtc], [IsDeleted])
                SELECT
                    NEWID(),
                    sv.FieldDefinitionId,
                    sv.[Value],
                    sv.SortOrder,
                    GETUTCDATE(),
                    0
                FROM SplitValues sv;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DropdownValues",
                schema: "Traceon");
        }
    }
}
