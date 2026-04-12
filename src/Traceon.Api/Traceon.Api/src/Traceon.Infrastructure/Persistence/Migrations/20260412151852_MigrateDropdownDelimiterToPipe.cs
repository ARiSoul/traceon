using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traceon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateDropdownDelimiterToPipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FieldDefinitions.DropdownValues: comma → pipe
            // Skip ref: rows (CompositeDropdown GUID references stay comma-separated)
            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldDefinitions]
                SET [DropdownValues] = REPLACE([DropdownValues], ',', '|')
                WHERE [DropdownValues] IS NOT NULL
                  AND [DropdownValues] <> ''
                  AND [DropdownValues] NOT LIKE 'ref:%'
                """);

            // FieldDependencyRules.TargetConstraint: comma → pipe
            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldDependencyRules]
                SET [TargetConstraint] = REPLACE([TargetConstraint], ',', '|')
                WHERE [TargetConstraint] IS NOT NULL
                  AND [TargetConstraint] <> ''
                """);

            // FieldAnalyticsRules.NegativeValues: comma → pipe
            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldAnalyticsRules]
                SET [NegativeValues] = REPLACE([NegativeValues], ',', '|')
                WHERE [NegativeValues] IS NOT NULL
                  AND [NegativeValues] <> ''
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldDefinitions]
                SET [DropdownValues] = REPLACE([DropdownValues], '|', ',')
                WHERE [DropdownValues] IS NOT NULL
                  AND [DropdownValues] <> ''
                  AND [DropdownValues] NOT LIKE 'ref:%'
                """);

            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldDependencyRules]
                SET [TargetConstraint] = REPLACE([TargetConstraint], '|', ',')
                WHERE [TargetConstraint] IS NOT NULL
                  AND [TargetConstraint] <> ''
                """);

            migrationBuilder.Sql("""
                UPDATE [Traceon].[FieldAnalyticsRules]
                SET [NegativeValues] = REPLACE([NegativeValues], '|', ',')
                WHERE [NegativeValues] IS NOT NULL
                  AND [NegativeValues] <> ''
                """);
        }
    }
}