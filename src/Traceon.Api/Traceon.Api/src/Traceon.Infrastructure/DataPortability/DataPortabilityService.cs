using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure.DataPortability;

public sealed class DataPortabilityService(TraceonDbContext db)
{
    public async Task<UserDataExport> ExportAsync(string userId, string email)
    {
        var tags = await db.Tags
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var fieldDefs = await db.FieldDefinitions
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.DefaultName)
            .ToListAsync();

        var actions = await db.TrackedActions
            .Where(a => a.UserId == userId)
            .Include(a => a.Tags)
            .Include(a => a.Fields)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        var actionIds = actions.Select(a => a.Id).ToList();

        var entries = await db.ActionEntries
            .Where(e => actionIds.Contains(e.TrackedActionId))
            .Include(e => e.Fields)
            .OrderBy(e => e.OccurredAtUtc)
            .ToListAsync();

        var entriesByAction = entries.GroupBy(e => e.TrackedActionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new UserDataExport
        {
            Email = email,
            Tags = tags.Select(t => new TagExport
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color,
                CreatedAtUtc = t.CreatedAtUtc
            }).ToList(),
            FieldDefinitions = fieldDefs.Select(f => new FieldDefinitionExport
            {
                Id = f.Id,
                DefaultName = f.DefaultName,
                DefaultDescription = f.DefaultDescription,
                Type = f.Type,
                DropdownValues = f.DropdownValues,
                DefaultMaxValue = f.DefaultMaxValue,
                DefaultMinValue = f.DefaultMinValue,
                DefaultIsRequired = f.DefaultIsRequired,
                DefaultValue = f.DefaultValue,
                Unit = f.Unit,
                CreatedAtUtc = f.CreatedAtUtc
            }).ToList(),
            TrackedActions = actions.Select(a => new TrackedActionExport
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                SortOrder = a.SortOrder,
                CreatedAtUtc = a.CreatedAtUtc,
                TagIds = a.Tags.Select(t => t.TagId).ToList(),
                Fields = a.Fields.Select(f => new ActionFieldExport
                {
                    Id = f.Id,
                    FieldDefinitionId = f.FieldDefinitionId,
                    Name = f.Name,
                    Description = f.Description,
                    MaxValue = f.MaxValue,
                    MinValue = f.MinValue,
                    IsRequired = f.IsRequired,
                    DefaultValue = f.DefaultValue,
                    Unit = f.Unit,
                    Order = f.Order,
                    SummaryMetrics = f.SummaryMetrics,
                    TrendAggregation = f.TrendAggregation,
                    TrendChartType = f.TrendChartType,
                    TargetValue = f.TargetValue,
                    InitialValueBehavior = f.InitialValueBehavior,
                    InitialValuePeriodUnit = f.InitialValuePeriodUnit,
                    InitialValuePeriodCount = f.InitialValuePeriodCount
                }).OrderBy(f => f.Order).ToList(),
                Entries = entriesByAction.GetValueOrDefault(a.Id, [])
                    .Select(e => new ActionEntryExport
                    {
                        Id = e.Id,
                        OccurredAtUtc = e.OccurredAtUtc,
                        Notes = e.Notes,
                        CreatedAtUtc = e.CreatedAtUtc,
                        Fields = e.Fields.Select(ef => new EntryFieldExport
                        {
                            ActionFieldId = ef.ActionFieldId,
                            Value = ef.Value
                        }).ToList()
                    }).ToList()
            }).ToList()
        };
    }

    public async Task<ImportResult> ImportAsync(string userId, UserDataExport data)
    {
        var result = new ImportResult();

        // Maps from old exported IDs → IDs in this account (new or existing)
        var tagIdMap = new Dictionary<Guid, Guid>();
        var fieldDefIdMap = new Dictionary<Guid, Guid>();
        var actionFieldIdMap = new Dictionary<Guid, Guid>();

        // Load existing tags and field definitions for this user to detect duplicates
        var existingTags = await db.Tags
            .Where(t => t.UserId == userId)
            .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var existingFieldDefs = await db.FieldDefinitions
            .Where(f => f.UserId == userId)
            .ToDictionaryAsync(f => f.DefaultName, StringComparer.OrdinalIgnoreCase);

        // 1. Import tags — reuse existing by name
        foreach (var tagExport in data.Tags)
        {
            if (existingTags.TryGetValue(tagExport.Name, out var existing))
            {
                tagIdMap[tagExport.Id] = existing.Id;
                result.TagsSkipped++;
            }
            else
            {
                var tag = Tag.Create(userId, tagExport.Name, tagExport.Description, tagExport.Color);
                db.Tags.Add(tag);
                tagIdMap[tagExport.Id] = tag.Id;
                existingTags[tagExport.Name] = tag;
                result.TagsImported++;
            }
        }

        // 2. Import field definitions — reuse existing by name
        foreach (var fdExport in data.FieldDefinitions)
        {
            if (existingFieldDefs.TryGetValue(fdExport.DefaultName, out var existing))
            {
                fieldDefIdMap[fdExport.Id] = existing.Id;
                result.FieldDefinitionsSkipped++;
            }
            else
            {
                var fd = FieldDefinition.Create(
                    userId, fdExport.DefaultName, fdExport.Type,
                    fdExport.DefaultDescription, fdExport.DropdownValues,
                    fdExport.DefaultMaxValue, fdExport.DefaultMinValue,
                    fdExport.DefaultIsRequired, fdExport.DefaultValue, fdExport.Unit);
                db.FieldDefinitions.Add(fd);
                fieldDefIdMap[fdExport.Id] = fd.Id;
                existingFieldDefs[fdExport.DefaultName] = fd;
                result.FieldDefinitionsImported++;
            }
        }

        await db.SaveChangesAsync();

        // 3. Import tracked actions with fields, tags, and entries
        var existingActionNames = (await db.TrackedActions
            .Where(a => a.UserId == userId)
            .Select(a => a.Name)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var actionExport in data.TrackedActions)
        {
            // Deduplicate action name by appending a suffix
            var actionName = actionExport.Name;
            if (existingActionNames.Contains(actionName))
            {
                var suffix = 2;
                string candidate;
                do
                {
                    candidate = $"{actionExport.Name} ({suffix})";
                    suffix++;
                } while (existingActionNames.Contains(candidate));
                actionName = candidate;
                result.ActionsRenamed++;
            }

            existingActionNames.Add(actionName);

            var action = TrackedAction.Create(userId, actionName, actionExport.Description, actionExport.SortOrder);
            db.TrackedActions.Add(action);
            await db.SaveChangesAsync();

            // Tags
            foreach (var oldTagId in actionExport.TagIds)
            {
                if (tagIdMap.TryGetValue(oldTagId, out var newTagId))
                {
                    db.TrackedActionTags.Add(TrackedActionTag.Create(action.Id, newTagId));
                }
            }

            // Fields — skip if the field definition wasn't part of the export
            foreach (var fieldExport in actionExport.Fields)
            {
                if (!fieldDefIdMap.TryGetValue(fieldExport.FieldDefinitionId, out var newFieldDefId))
                    continue;

                var field = ActionField.Create(
                    action.Id, newFieldDefId, fieldExport.Name, fieldExport.Description,
                    fieldExport.MaxValue, fieldExport.MinValue, fieldExport.IsRequired,
                    fieldExport.DefaultValue, fieldExport.Unit, fieldExport.Order,
                    fieldExport.SummaryMetrics, fieldExport.TrendAggregation,
                    fieldExport.TrendChartType, fieldExport.TargetValue,
                    fieldExport.InitialValueBehavior, fieldExport.InitialValuePeriodUnit,
                    fieldExport.InitialValuePeriodCount);
                db.ActionFields.Add(field);
                actionFieldIdMap[fieldExport.Id] = field.Id;
            }

            await db.SaveChangesAsync();

            // Remap DropdownTrendValueFieldId references to new IDs
            foreach (var fieldExport2 in actionExport.Fields)
            {
                if (fieldExport2.DropdownTrendValueFieldId.HasValue
                    && actionFieldIdMap.TryGetValue(fieldExport2.Id, out var newSelfId)
                    && actionFieldIdMap.TryGetValue(fieldExport2.DropdownTrendValueFieldId.Value, out var newRefId))
                {
                    var tracked = await db.ActionFields.FindAsync(newSelfId);
                    tracked?.Update(tracked.Name, dropdownTrendValueFieldId: newRefId);
                }
            }
            await db.SaveChangesAsync();

            // Entries — add field values directly to the DbSet to avoid
            // change-tracker conflicts with the entry's private backing field
            foreach (var entryExport in actionExport.Entries)
            {
                var entry = ActionEntry.Create(action.Id, entryExport.OccurredAtUtc, entryExport.Notes);
                db.ActionEntries.Add(entry);
                await db.SaveChangesAsync();

                foreach (var efExport in entryExport.Fields)
                {
                    if (actionFieldIdMap.TryGetValue(efExport.ActionFieldId, out var newFieldId))
                    {
                        db.ActionEntryFields.Add(ActionEntryField.Create(entry.Id, newFieldId, efExport.Value));
                    }
                }

                await db.SaveChangesAsync();
                result.EntriesImported++;
            }

            result.ActionsImported++;
        }

        return result;
    }
}

public sealed class ImportResult
{
    public int TagsImported { get; set; }
    public int TagsSkipped { get; set; }
    public int FieldDefinitionsImported { get; set; }
    public int FieldDefinitionsSkipped { get; set; }
    public int ActionsImported { get; set; }
    public int ActionsRenamed { get; set; }
    public int EntriesImported { get; set; }
}
