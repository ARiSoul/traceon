var apiRoot = @"C:\Users\rjrso\source\repos\traceon\src\Traceon.Api\Traceon.Api\src";

// ─── 1. Domain Entity: DropdownValue.cs ───
var entityPath = Path.Combine(apiRoot, "Traceon.Domain", "Entities", "DropdownValue.cs");
File.WriteAllText(entityPath, """
namespace Traceon.Domain.Entities;

public sealed class DropdownValue : Entity
{
    public Guid FieldDefinitionId { get; private set; }
    public string Value { get; private set; }
    public int SortOrder { get; private set; }

    private DropdownValue(Guid fieldDefinitionId, string value, int sortOrder)
    {
        FieldDefinitionId = fieldDefinitionId;
        Value = value;
        SortOrder = sortOrder;
    }

    public static DropdownValue Create(Guid fieldDefinitionId, string value, int sortOrder = 0)
    {
        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition ID is required.", nameof(fieldDefinitionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new DropdownValue(fieldDefinitionId, value.Trim(), sortOrder);
    }

    public void Rename(string newValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newValue);
        Value = newValue.Trim();
        MarkUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        MarkUpdated();
    }
}
""");
Console.WriteLine($"Created: {entityPath}");

// ─── 2. Repository Interface: IDropdownValueRepository.cs ───
var repoInterfacePath = Path.Combine(apiRoot, "Traceon.Domain", "Repositories", "IDropdownValueRepository.cs");
File.WriteAllText(repoInterfacePath, """
using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IDropdownValueRepository
{
    Task<DropdownValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdsAsync(IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default);
    Task AddAsync(DropdownValue entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<DropdownValue> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(DropdownValue entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cascades a value rename across all related tables in a single transaction.
    /// </summary>
    Task CascadeRenameAsync(
        Guid fieldDefinitionId,
        string oldValue,
        string newValue,
        CancellationToken cancellationToken = default);
}
""");
Console.WriteLine($"Created: {repoInterfacePath}");

// ─── 3. EF Configuration: DropdownValueConfiguration.cs ───
var configPath = Path.Combine(apiRoot, "Traceon.Infrastructure", "Persistence", "Configurations", "DropdownValueConfiguration.cs");
File.WriteAllText(configPath, """
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class DropdownValueConfiguration : IEntityTypeConfiguration<DropdownValue>
{
    public void Configure(EntityTypeBuilder<DropdownValue> builder)
    {
        builder.ToTable("DropdownValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.FieldDefinitionId);

        builder.HasIndex(e => new { e.FieldDefinitionId, e.Value })
            .IsUnique();
    }
}
""");
Console.WriteLine($"Created: {configPath}");

// ─── 4. Repository Implementation: DropdownValueRepository.cs ───
var repoImplPath = Path.Combine(apiRoot, "Traceon.Infrastructure", "Persistence", "Repositories", "DropdownValueRepository.cs");
File.WriteAllText(repoImplPath, """
using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class DropdownValueRepository(TraceonDbContext context) : IDropdownValueRepository
{
    public async Task<DropdownValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.DropdownValues.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        return await context.DropdownValues
            .AsNoTracking()
            .Where(dv => dv.FieldDefinitionId == fieldDefinitionId)
            .OrderBy(dv => dv.SortOrder)
            .ThenBy(dv => dv.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdsAsync(
        IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default)
    {
        var ids = fieldDefinitionIds.ToList();
        return await context.DropdownValues
            .AsNoTracking()
            .Where(dv => ids.Contains(dv.FieldDefinitionId))
            .OrderBy(dv => dv.SortOrder)
            .ThenBy(dv => dv.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DropdownValue entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<DropdownValue> entities, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.AddRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DropdownValue entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.DropdownValues
            .Where(dv => dv.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        await context.DropdownValues
            .Where(dv => dv.FieldDefinitionId == fieldDefinitionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CascadeRenameAsync(
        Guid fieldDefinitionId,
        string oldValue,
        string newValue,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Find all ActionField IDs that use this FieldDefinition
            var actionFieldIds = await context.ActionFields
                .AsNoTracking()
                .Where(af => af.FieldDefinitionId == fieldDefinitionId)
                .Select(af => af.Id)
                .ToListAsync(cancellationToken);

            if (actionFieldIds.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            // 2. Cascade to ActionEntryFields
            await context.ActionEntryFields
                .Where(aef => actionFieldIds.Contains(aef.ActionFieldId) && aef.Value == oldValue)
                .ExecuteUpdateAsync(s => s.SetProperty(aef => aef.Value, newValue), cancellationToken);

            // 3. Cascade to FieldDependencyRules — SourceValue
            await context.FieldDependencyRules
                .Where(r => actionFieldIds.Contains(r.SourceFieldId) && r.SourceValue == oldValue)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.SourceValue, newValue), cancellationToken);

            // 4. Cascade to FieldDependencyRules — TargetConstraint (pipe-delimited)
            var targetDepRules = await context.FieldDependencyRules
                .Where(r => actionFieldIds.Contains(r.TargetFieldId)
                         && r.TargetConstraint != null
                         && r.TargetConstraint.Contains(oldValue))
                .ToListAsync(cancellationToken);

            foreach (var rule in targetDepRules)
            {
                var values = rule.TargetConstraint!.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updated = values.Select(v => v == oldValue ? newValue : v);
                rule.Update(targetConstraint: string.Join('|', updated));
            }

            // 5. Cascade to ReceiptMappingRules
            var receiptConfigIds = await context.ReceiptImportConfigs
                .AsNoTracking()
                .Select(c => new { c.Id, c.TrackedActionId })
                .ToListAsync(cancellationToken);

            // Get TrackedActionIds that have any of our ActionFields
            var trackedActionIds = await context.ActionFields
                .AsNoTracking()
                .Where(af => af.FieldDefinitionId == fieldDefinitionId)
                .Select(af => af.TrackedActionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var relevantConfigIds = receiptConfigIds
                .Where(c => trackedActionIds.Contains(c.TrackedActionId))
                .Select(c => c.Id)
                .ToList();

            if (relevantConfigIds.Count > 0)
            {
                await context.ReceiptMappingRules
                    .Where(r => relevantConfigIds.Contains(r.ReceiptImportConfigId)
                             && actionFieldIds.Contains(r.TargetFieldId)
                             && r.Value == oldValue)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Value, newValue), cancellationToken);
            }

            // 6. Cascade to FieldAnalyticsRules — FilterValue
            await context.FieldAnalyticsRules
                .Where(r => trackedActionIds.Contains(r.TrackedActionId)
                         && ((actionFieldIds.Contains(r.MeasureFieldId) && r.FilterValue == oldValue)
                          || (r.FilterFieldId.HasValue && actionFieldIds.Contains(r.FilterFieldId.Value) && r.FilterValue == oldValue)))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.FilterValue, newValue), cancellationToken);

            // 7. Cascade to FieldAnalyticsRules — NegativeValues (comma-delimited)
            var analyticsWithNegValues = await context.FieldAnalyticsRules
                .Where(r => trackedActionIds.Contains(r.TrackedActionId)
                         && r.NegativeValues != null
                         && r.NegativeValues.Contains(oldValue))
                .ToListAsync(cancellationToken);

            foreach (var rule in analyticsWithNegValues)
            {
                var values = rule.NegativeValues!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updated = values.Select(v => v == oldValue ? newValue : v);
                rule.Update(negativeValues: string.Join(',', updated));
            }

            // 8. Cascade to ConnectedActionRules — ConditionsJson & FieldMappingsJson
            var connRules = await context.ConnectedActionRules
                .Where(r => (trackedActionIds.Contains(r.SourceTrackedActionId) || trackedActionIds.Contains(r.TargetTrackedActionId))
                         && ((r.ConditionsJson != null && r.ConditionsJson.Contains(oldValue))
                          || (r.FieldMappingsJson != null && r.FieldMappingsJson.Contains(oldValue))))
                .ToListAsync(cancellationToken);

            foreach (var rule in connRules)
            {
                var newConditions = rule.ConditionsJson?.Replace(oldValue, newValue);
                var newMappings = rule.FieldMappingsJson?.Replace(oldValue, newValue);
                rule.Update(
                    conditionsJson: newConditions,
                    fieldMappingsJson: newMappings);
            }

            // 9. Update the FieldDefinition.DropdownValues pipe-delimited string
            var fieldDef = await context.FieldDefinitions.FindAsync([fieldDefinitionId], cancellationToken);
            if (fieldDef is not null && fieldDef.DropdownValues is not null)
            {
                var ddValues = fieldDef.DropdownValues.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updatedDdValues = ddValues.Select(v => v == oldValue ? newValue : v);
                var newDdString = string.Join('|', updatedDdValues);
                fieldDef.Update(
                    fieldDef.DefaultName,
                    fieldDef.Type,
                    fieldDef.DefaultDescription,
                    newDdString,
                    fieldDef.DefaultMaxValue,
                    fieldDef.DefaultMinValue,
                    fieldDef.DefaultIsRequired,
                    fieldDef.DefaultValue == oldValue ? newValue : fieldDef.DefaultValue,
                    fieldDef.Unit);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
""");
Console.WriteLine($"Created: {repoImplPath}");

// ─── 5. Contracts ───
var contractsDir = Path.Combine(apiRoot, "Traceon.Contracts", "DropdownValues");
Directory.CreateDirectory(contractsDir);

File.WriteAllText(Path.Combine(contractsDir, "DropdownValueResponse.cs"), """
namespace Traceon.Contracts.DropdownValues;

public sealed record DropdownValueResponse
{
    public required Guid Id { get; init; }
    public required Guid FieldDefinitionId { get; init; }
    public required string Value { get; init; }
    public required int SortOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
""");
Console.WriteLine($"Created: DropdownValueResponse.cs");

File.WriteAllText(Path.Combine(contractsDir, "RenameDropdownValueRequest.cs"), """
namespace Traceon.Contracts.DropdownValues;

public sealed record RenameDropdownValueRequest(string NewValue);
""");
Console.WriteLine($"Created: RenameDropdownValueRequest.cs");

File.WriteAllText(Path.Combine(contractsDir, "ReorderDropdownValuesRequest.cs"), """
namespace Traceon.Contracts.DropdownValues;

public sealed record ReorderDropdownValuesRequest(List<Guid> OrderedIds);
""");
Console.WriteLine($"Created: ReorderDropdownValuesRequest.cs");

// ─── 6. Service Interface ───
File.WriteAllText(Path.Combine(apiRoot, "Traceon.Application", "Services", "IDropdownValueService.cs"), """
using Traceon.Application.Common;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Application.Services;

public interface IDropdownValueService
{
    Task<Result<IReadOnlyList<DropdownValueResponse>>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<Result<DropdownValueResponse>> RenameAsync(Guid id, string newValue, CancellationToken cancellationToken = default);
    Task<Result> ReorderAsync(Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Syncs the DropdownValues table from the FieldDefinition.DropdownValues pipe-delimited string.
    /// Called during migration or when the legacy append API is used.
    /// </summary>
    Task<Result> SyncFromFieldDefinitionAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
}
""");
Console.WriteLine("Created: IDropdownValueService.cs");

// ─── 7. Service Implementation ───
File.WriteAllText(Path.Combine(apiRoot, "Traceon.Application", "Services", "DropdownValueService.cs"), """
using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.DropdownValues;
using Traceon.Contracts.Helpers;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class DropdownValueService(
    IDropdownValueRepository repository,
    IFieldDefinitionRepository fieldDefinitionRepository,
    ICurrentUserService currentUser,
    ILogger<DropdownValueService> logger) : IDropdownValueService
{
    public async Task<Result<IReadOnlyList<DropdownValueResponse>>> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<IReadOnlyList<DropdownValueResponse>>.Failure("Field definition not found.");

        var entities = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var responses = entities.Select(ToResponse).ToList() as IReadOnlyList<DropdownValueResponse>;
        return Result<IReadOnlyList<DropdownValueResponse>>.Success(responses);
    }

    public async Task<Result<DropdownValueResponse>> RenameAsync(
        Guid id, string newValue, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<DropdownValueResponse>.Failure("Dropdown value not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<DropdownValueResponse>.Failure("Dropdown value not found.");

        var trimmed = newValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return Result<DropdownValueResponse>.Failure("Value cannot be empty.");

        if (trimmed == entity.Value)
            return Result<DropdownValueResponse>.Success(ToResponse(entity));

        // Check for duplicate
        var siblings = await repository.GetByFieldDefinitionIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (siblings.Any(s => s.Id != id && string.Equals(s.Value, trimmed, StringComparison.OrdinalIgnoreCase)))
            return Result<DropdownValueResponse>.Failure("A dropdown value with this name already exists.");

        var oldValue = entity.Value;

        // Cascade rename across all related tables
        await repository.CascadeRenameAsync(entity.FieldDefinitionId, oldValue, trimmed, cancellationToken);

        // Rename the entity itself
        entity.Rename(trimmed);
        await repository.UpdateAsync(entity, cancellationToken);

        logger.LogInformation("Renamed dropdown value {Id} from '{OldValue}' to '{NewValue}' in FieldDefinition {FieldDefinitionId}",
            id, oldValue, trimmed, entity.FieldDefinitionId);

        return Result<DropdownValueResponse>.Success(ToResponse(entity));
    }

    public async Task<Result> ReorderAsync(
        Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Field definition not found.");

        var entities = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var entityMap = entities.ToDictionary(e => e.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (entityMap.TryGetValue(orderedIds[i], out var entity))
            {
                entity.SetSortOrder(i);
                await repository.UpdateAsync(entity, cancellationToken);
            }
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Dropdown value not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Dropdown value not found.");

        await repository.DeleteAsync(id, cancellationToken);

        // Rebuild the pipe-delimited string
        await RebuildDropdownValuesStringAsync(entity.FieldDefinitionId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SyncFromFieldDefinitionAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null)
            return Result.Failure("Field definition not found.");

        // Skip composite dropdowns (ref: prefix)
        if (fieldDef.DropdownValues?.StartsWith("ref:", StringComparison.Ordinal) == true)
            return Result.Success();

        var existingValues = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var existingSet = existingValues.ToDictionary(v => v.Value, StringComparer.OrdinalIgnoreCase);

        var pipeValues = DropdownValuesHelper.Split(fieldDef.DropdownValues);
        var order = 0;

        foreach (var val in pipeValues)
        {
            if (!existingSet.ContainsKey(val))
            {
                var entity = DropdownValue.Create(fieldDefinitionId, val, order);
                await repository.AddAsync(entity, cancellationToken);
            }
            else
            {
                var existing = existingSet[val];
                if (existing.SortOrder != order)
                {
                    existing.SetSortOrder(order);
                    await repository.UpdateAsync(existing, cancellationToken);
                }
            }
            order++;
        }

        // Remove values that are no longer in the pipe string
        var pipeSet = new HashSet<string>(pipeValues, StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingValues)
        {
            if (!pipeSet.Contains(existing.Value))
            {
                await repository.DeleteAsync(existing.Id, cancellationToken);
            }
        }

        return Result.Success();
    }

    private async Task RebuildDropdownValuesStringAsync(Guid fieldDefinitionId, CancellationToken cancellationToken)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null) return;

        var values = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var newPipeString = DropdownValuesHelper.Join(values.Select(v => v.Value));

        fieldDef.Update(
            fieldDef.DefaultName,
            fieldDef.Type,
            fieldDef.DefaultDescription,
            string.IsNullOrEmpty(newPipeString) ? null : newPipeString,
            fieldDef.DefaultMaxValue,
            fieldDef.DefaultMinValue,
            fieldDef.DefaultIsRequired,
            fieldDef.DefaultValue,
            fieldDef.Unit);

        await fieldDefinitionRepository.UpdateAsync(fieldDef, cancellationToken);
    }

    private static DropdownValueResponse ToResponse(DropdownValue entity) => new()
    {
        Id = entity.Id,
        FieldDefinitionId = entity.FieldDefinitionId,
        Value = entity.Value,
        SortOrder = entity.SortOrder,
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
    };
}
""");
Console.WriteLine("Created: DropdownValueService.cs");

// ─── 8. API Endpoints ───
File.WriteAllText(Path.Combine(apiRoot, "Traceon.Api", "Endpoints", "DropdownValueEndpoints.cs"), """
using Traceon.Api.Extensions;
using Traceon.Application.Services;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Api.Endpoints;

internal static class DropdownValueEndpoints
{
    public static RouteGroupBuilder MapDropdownValueEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/dropdown-values")
            .WithTags("Dropdown Values");

        group.MapGet("/by-field/{fieldDefinitionId:guid}", GetByFieldDefinitionIdAsync);
        group.MapPut("/{id:guid}/rename", RenameAsync);
        group.MapPut("/by-field/{fieldDefinitionId:guid}/reorder", ReorderAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/by-field/{fieldDefinitionId:guid}/sync", SyncAsync);

        return group;
    }

    private static async Task<IResult> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RenameAsync(
        Guid id,
        RenameDropdownValueRequest request,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.RenameAsync(id, request.NewValue, cancellationToken)).ToHttpResult();

    private static async Task<IResult> ReorderAsync(
        Guid fieldDefinitionId,
        ReorderDropdownValuesRequest request,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.ReorderAsync(fieldDefinitionId, request.OrderedIds, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> SyncAsync(
        Guid fieldDefinitionId,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.SyncFromFieldDefinitionAsync(fieldDefinitionId, cancellationToken)).ToHttpResult();
}
""");
Console.WriteLine("Created: DropdownValueEndpoints.cs");

Console.WriteLine("\n✅ All new files created successfully!");
