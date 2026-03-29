using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure.Onboarding;

public sealed class TemplateInstallService(TraceonDbContext db)
{
    public async Task<TemplateInstallResult> InstallAsync(string userId, TemplatePack pack)
    {
        var result = new TemplateInstallResult();

        // Reuse existing tags by name (same pattern as import)
        var existingTags = await db.Tags
            .Where(t => t.UserId == userId)
            .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var tagIds = new List<Guid>();
        foreach (var tagTemplate in pack.Tags)
        {
            if (existingTags.TryGetValue(tagTemplate.Name, out var existing))
            {
                tagIds.Add(existing.Id);
            }
            else
            {
                var tag = Tag.Create(userId, tagTemplate.Name, color: tagTemplate.Color);
                db.Tags.Add(tag);
                tagIds.Add(tag.Id);
                existingTags[tagTemplate.Name] = tag;
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
            var actionName = actionTemplate.Name;
            if (existingActionNames.Contains(actionName))
            {
                var suffix = 2;
                string candidate;
                do { candidate = $"{actionName} ({suffix++})"; }
                while (existingActionNames.Contains(candidate));
                actionName = candidate;
            }
            existingActionNames.Add(actionName);

            var action = TrackedAction.Create(userId, actionName, actionTemplate.Description, sortOrder++);
            db.TrackedActions.Add(action);
            await db.SaveChangesAsync();

            // Assign tags
            foreach (var tagId in tagIds)
            {
                db.TrackedActionTags.Add(TrackedActionTag.Create(action.Id, tagId));
            }

            // Create fields
            foreach (var fieldTemplate in actionTemplate.Fields)
            {
                // Reuse or create field definition
                Guid fieldDefId;
                if (existingFieldDefs.TryGetValue(fieldTemplate.Name, out var existingDef))
                {
                    fieldDefId = existingDef.Id;
                }
                else
                {
                    var fd = FieldDefinition.Create(
                        userId, fieldTemplate.Name, fieldTemplate.Type,
                        unit: fieldTemplate.Unit,
                        defaultMaxValue: fieldTemplate.MaxValue,
                        defaultMinValue: fieldTemplate.MinValue,
                        defaultIsRequired: fieldTemplate.IsRequired,
                        defaultValue: fieldTemplate.DefaultValue,
                        dropdownValues: fieldTemplate.DropdownValues);
                    db.FieldDefinitions.Add(fd);
                    existingFieldDefs[fieldTemplate.Name] = fd;
                    fieldDefId = fd.Id;
                    result.FieldDefinitionsCreated++;
                }

                var field = ActionField.Create(
                    action.Id, fieldDefId, fieldTemplate.Name,
                    maxValue: fieldTemplate.MaxValue,
                    minValue: fieldTemplate.MinValue,
                    isRequired: fieldTemplate.IsRequired,
                    defaultValue: fieldTemplate.DefaultValue,
                    unit: fieldTemplate.Unit,
                    order: fieldTemplate.Order,
                    targetValue: fieldTemplate.TargetValue);
                db.ActionFields.Add(field);
            }

            await db.SaveChangesAsync();
            result.ActionsCreated++;
        }

        return result;
    }
}

public sealed class TemplateInstallResult
{
    public int TagsCreated { get; set; }
    public int FieldDefinitionsCreated { get; set; }
    public int ActionsCreated { get; set; }
}
