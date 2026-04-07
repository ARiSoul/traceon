using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure.Onboarding;

public sealed class TemplateInstallService(TraceonDbContext db)
{
    public async Task<TemplateInstallResult> InstallAsync(string userId, TemplatePack pack, string? language = null)
    {
        var result = new TemplateInstallResult();
        string L(string? key, string fallback) => TemplateLocalization.Resolve(key, fallback, language);

        // Reuse existing tags by name (same pattern as import)
        var existingTags = await db.Tags
            .Where(t => t.UserId == userId)
            .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var tagIds = new List<Guid>();
        foreach (var tagTemplate in pack.Tags)
        {
            var tagName = L(tagTemplate.NameKey, tagTemplate.Name);
            if (existingTags.TryGetValue(tagName, out var existing))
            {
                tagIds.Add(existing.Id);
            }
            else
            {
                var tag = Tag.Create(userId, tagName, color: tagTemplate.Color);
                db.Tags.Add(tag);
                tagIds.Add(tag.Id);
                existingTags[tagName] = tag;
                result.TagsCreated++;
            }
        }

        await db.SaveChangesAsync();

        // Deduplicate action names
        var existingActionNames = (await db.TrackedActions
            .Where(a => a.UserId == userId)
            .Select(a => a.Name)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Reuse existing field definitions by name
        var existingFieldDefs = await db.FieldDefinitions
            .Where(f => f.UserId == userId)
            .ToDictionaryAsync(f => f.DefaultName, StringComparer.OrdinalIgnoreCase);

        var sortOrder = existingActionNames.Count;

        foreach (var actionTemplate in pack.Actions)
        {
            var actionName = L(actionTemplate.NameKey, actionTemplate.Name);
            if (existingActionNames.Contains(actionName))
            {
                var suffix = 2;
                string candidate;
                do { candidate = $"{actionName} ({suffix++})"; }
                while (existingActionNames.Contains(candidate));
                actionName = candidate;
            }
            existingActionNames.Add(actionName);

            var actionDescription = L(actionTemplate.DescriptionKey, actionTemplate.Description ?? "");
            var action = TrackedAction.Create(userId, actionName, actionDescription, sortOrder++);
            db.TrackedActions.Add(action);
            await db.SaveChangesAsync();

            // Assign tags
            foreach (var tagId in tagIds)
            {
                db.TrackedActionTags.Add(TrackedActionTag.Create(action.Id, tagId));
            }

            // Create fields
            var fieldIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            foreach (var fieldTemplate in actionTemplate.Fields)
            {
                var fieldName = L(fieldTemplate.NameKey, fieldTemplate.Name);
                var dropdownValues = L(fieldTemplate.DropdownValuesKey, fieldTemplate.DropdownValues ?? "");
                var resolvedDropdown = string.IsNullOrWhiteSpace(dropdownValues) ? fieldTemplate.DropdownValues : dropdownValues;

                // Reuse or create field definition
                Guid fieldDefId;
                if (existingFieldDefs.TryGetValue(fieldName, out var existingDef))
                {
                    fieldDefId = existingDef.Id;
                }
                else
                {
                    var fd = FieldDefinition.Create(
                        userId, fieldName, fieldTemplate.Type,
                        unit: fieldTemplate.Unit,
                        defaultMaxValue: fieldTemplate.MaxValue,
                        defaultMinValue: fieldTemplate.MinValue,
                        defaultIsRequired: fieldTemplate.IsRequired,
                        defaultValue: fieldTemplate.DefaultValue,
                        dropdownValues: resolvedDropdown);
                    db.FieldDefinitions.Add(fd);
                    existingFieldDefs[fieldName] = fd;
                    fieldDefId = fd.Id;
                    result.FieldDefinitionsCreated++;
                }

                var field = ActionField.Create(
                    action.Id, fieldDefId, fieldName,
                    maxValue: fieldTemplate.MaxValue,
                    minValue: fieldTemplate.MinValue,
                    isRequired: fieldTemplate.IsRequired,
                    defaultValue: fieldTemplate.DefaultValue,
                    unit: fieldTemplate.Unit,
                    order: fieldTemplate.Order,
                    targetValue: fieldTemplate.TargetValue);
                db.ActionFields.Add(field);
                fieldIdsByName[fieldTemplate.Name] = field.Id;
            }

            await db.SaveChangesAsync();

            // Resolve DropdownTrendValueFieldId references (requires fields to be persisted first)
            foreach (var fieldTemplate in actionTemplate.Fields)
            {
                if (fieldTemplate.DropdownTrendValueFieldName is not null
                    && fieldIdsByName.TryGetValue(fieldTemplate.Name, out var selfId)
                    && fieldIdsByName.TryGetValue(fieldTemplate.DropdownTrendValueFieldName, out var refId))
                {
                    var tracked = await db.ActionFields.FindAsync(selfId);
                    tracked?.Update(tracked.Name,
                        dropdownTrendValueFieldId: refId,
                        dropdownTrendAggregation: fieldTemplate.DropdownTrendAggregation,
                        dropdownTrendChartType: fieldTemplate.DropdownTrendChartType);
                }
            }
            await db.SaveChangesAsync();

            // Create analytics rules
            foreach (var ruleTemplate in actionTemplate.AnalyticsRules)
            {
                if (!fieldIdsByName.TryGetValue(ruleTemplate.MeasureFieldName, out var measureFieldId))
                    continue;
                if (!fieldIdsByName.TryGetValue(ruleTemplate.GroupByFieldName, out var groupByFieldId))
                    continue;

                Guid? signFieldId = null;
                if (ruleTemplate.SignFieldName is not null &&
                    fieldIdsByName.TryGetValue(ruleTemplate.SignFieldName, out var signId))
                {
                    signFieldId = signId;
                }

                Guid? filterFieldId = null;
                if (ruleTemplate.FilterFieldName is not null &&
                    fieldIdsByName.TryGetValue(ruleTemplate.FilterFieldName, out var filterId))
                {
                    filterFieldId = filterId;
                }

                // Resolve localized dropdown-based values
                var negativeValues = LocalizeDropdownValue(
                    ruleTemplate.NegativeValues, ruleTemplate.SignFieldName, actionTemplate.Fields, L, language);
                var filterValue = LocalizeDropdownValue(
                    ruleTemplate.FilterValue, ruleTemplate.FilterFieldName, actionTemplate.Fields, L, language);

                var label = L(ruleTemplate.LabelKey, ruleTemplate.Label ?? "");

                var rule = FieldAnalyticsRule.Create(
                    action.Id,
                    measureFieldId,
                    groupByFieldId,
                    aggregation: ruleTemplate.Aggregation,
                    displayType: ruleTemplate.DisplayType,
                    filterFieldId: filterFieldId,
                    filterValue: filterValue,
                    label: string.IsNullOrWhiteSpace(label) ? ruleTemplate.Label : label,
                    sortOrder: ruleTemplate.SortOrder,
                    signFieldId: signFieldId,
                    negativeValues: negativeValues);
                db.FieldAnalyticsRules.Add(rule);
                result.AnalyticsRulesCreated++;
            }

            await db.SaveChangesAsync();
            result.ActionsCreated++;
        }

        return result;
    }

    /// <summary>
    /// Localizes a comma-separated value string (e.g. "Expense") by mapping each part
    /// through the referenced field's dropdown localization.
    /// </summary>
    private static string? LocalizeDropdownValue(
        string? value, string? fieldName, List<FieldTemplate> fields,
        Func<string?, string, string> L, string? language)
    {
        if (value is null || language is null || fieldName is null)
            return value;

        var field = fields.FirstOrDefault(f => f.Name == fieldName);
        if (field?.DropdownValuesKey is null)
            return value;

        var localizedDropdown = L(field.DropdownValuesKey, field.DropdownValues ?? "");
        var originalValues = (field.DropdownValues ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var localizedValues = localizedDropdown.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (originalValues.Length != localizedValues.Length)
            return value;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < originalValues.Length; i++)
            map[originalValues[i]] = localizedValues[i];

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(",", parts.Select(p => map.GetValueOrDefault(p, p)));
    }
}

public sealed class TemplateInstallResult
{
    public int TagsCreated { get; set; }
    public int FieldDefinitionsCreated { get; set; }
    public int ActionsCreated { get; set; }
    public int AnalyticsRulesCreated { get; set; }
}
