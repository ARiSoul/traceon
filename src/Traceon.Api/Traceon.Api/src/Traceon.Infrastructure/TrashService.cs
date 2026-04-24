using Microsoft.EntityFrameworkCore;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure;

public sealed record DeletedItemResponse(
    Guid Id,
    string Type,
    string Name,
    DateTime DeletedAtUtc,
    Guid? ParentId = null);

public sealed record TrashItemRef(string Type, Guid Id);

public sealed record TrashPreviewResponse(
    Guid Id,
    string Type,
    string Name,
    DateTime CreatedAtUtc,
    DateTime DeletedAtUtc,
    IReadOnlyList<TrashPreviewDetail> Details);

public sealed record TrashPreviewDetail(string Key, string? Value);

public sealed class TrashService(TraceonDbContext context, ICurrentUserService currentUser)
{
    public async Task<List<DeletedItemResponse>> GetDeletedItemsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var actions = await context.TrackedActions
            .IgnoreQueryFilters()
            .Where(a => a.UserId == userId && a.IsDeleted && a.DeletedAtUtc != null)
            .Select(a => new DeletedItemResponse(a.Id, "TrackedAction", a.Name, a.DeletedAtUtc!.Value, null))
            .ToListAsync(cancellationToken);

        var fieldDefs = await context.FieldDefinitions
            .IgnoreQueryFilters()
            .Where(fd => fd.UserId == userId && fd.IsDeleted && fd.DeletedAtUtc != null)
            .Select(fd => new DeletedItemResponse(fd.Id, "FieldDefinition", fd.DefaultName, fd.DeletedAtUtc!.Value, null))
            .ToListAsync(cancellationToken);

        var tags = await context.Tags
            .IgnoreQueryFilters()
            .Where(t => t.UserId == userId && t.IsDeleted && t.DeletedAtUtc != null)
            .Select(t => new DeletedItemResponse(t.Id, "Tag", t.Name, t.DeletedAtUtc!.Value, null))
            .ToListAsync(cancellationToken);

        var entries = await context.ActionEntries
            .IgnoreQueryFilters()
            .Where(e => e.IsDeleted && e.DeletedAtUtc != null
                && context.TrackedActions.Any(a => a.Id == e.TrackedActionId && a.UserId == userId))
            .Select(e => new DeletedItemResponse(
                e.Id,
                "ActionEntry",
                context.TrackedActions.Where(a => a.Id == e.TrackedActionId).Select(a => a.Name).FirstOrDefault() + " — " + e.OccurredAtUtc.ToString("yyyy-MM-dd HH:mm"),
                e.DeletedAtUtc!.Value,
                e.TrackedActionId))
            .ToListAsync(cancellationToken);

        var fields = await context.ActionFields
            .IgnoreQueryFilters()
            .Where(f => f.IsDeleted && f.DeletedAtUtc != null
                && context.TrackedActions.Any(a => a.Id == f.TrackedActionId && a.UserId == userId))
            .Select(f => new DeletedItemResponse(
                f.Id,
                "ActionField",
                context.TrackedActions.Where(a => a.Id == f.TrackedActionId).Select(a => a.Name).FirstOrDefault() + " — " + f.Name,
                f.DeletedAtUtc!.Value,
                f.TrackedActionId))
            .ToListAsync(cancellationToken);

        return [.. actions, .. fieldDefs, .. tags, .. entries, .. fields];
    }

    public async Task<Result<TrashPreviewResponse>> GetPreviewAsync(
        string type, Guid id, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        switch (type)
        {
            case "TrackedAction":
            {
                var a = await context.TrackedActions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.IsDeleted, cancellationToken);

                if (a is null)
                    return Result<TrashPreviewResponse>.Failure("Item not found in trash.", ResultErrorType.NotFound);

                var fieldCount = await context.ActionFields.IgnoreQueryFilters()
                    .CountAsync(f => f.TrackedActionId == id, cancellationToken);
                var entryCount = await context.ActionEntries.IgnoreQueryFilters()
                    .CountAsync(e => e.TrackedActionId == id, cancellationToken);

                return Result<TrashPreviewResponse>.Success(new TrashPreviewResponse(
                    a.Id, type, a.Name, a.CreatedAtUtc, a.DeletedAtUtc!.Value,
                    [
                        new("Description", a.Description),
                        new("Fields", fieldCount.ToString()),
                        new("Entries", entryCount.ToString())
                    ]));
            }
            case "FieldDefinition":
            {
                var fd = await context.FieldDefinitions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.IsDeleted, cancellationToken);

                if (fd is null)
                    return Result<TrashPreviewResponse>.Failure("Item not found in trash.", ResultErrorType.NotFound);

                var refCount = await context.ActionFields.IgnoreQueryFilters()
                    .CountAsync(f => f.FieldDefinitionId == id, cancellationToken);

                return Result<TrashPreviewResponse>.Success(new TrashPreviewResponse(
                    fd.Id, type, fd.DefaultName, fd.CreatedAtUtc, fd.DeletedAtUtc!.Value,
                    [
                        new("Type", fd.Type.ToString()),
                        new("Unit", fd.Unit),
                        new("Description", fd.DefaultDescription),
                        new("DropdownValues", fd.DropdownValues),
                        new("References", refCount.ToString())
                    ]));
            }
            case "Tag":
            {
                var t = await context.Tags
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.IsDeleted, cancellationToken);

                if (t is null)
                    return Result<TrashPreviewResponse>.Failure("Item not found in trash.", ResultErrorType.NotFound);

                return Result<TrashPreviewResponse>.Success(new TrashPreviewResponse(
                    t.Id, type, t.Name, t.CreatedAtUtc, t.DeletedAtUtc!.Value,
                    [
                        new("Color", t.Color)
                    ]));
            }
            case "ActionEntry":
            {
                var e = await context.ActionEntries
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted
                        && context.TrackedActions.Any(a => a.Id == x.TrackedActionId && a.UserId == userId), cancellationToken);

                if (e is null)
                    return Result<TrashPreviewResponse>.Failure("Item not found in trash.", ResultErrorType.NotFound);

                var actionName = await context.TrackedActions
                    .Where(a => a.Id == e.TrackedActionId)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "";

                var fieldValueCount = await context.ActionEntryFields.IgnoreQueryFilters()
                    .CountAsync(f => f.ActionEntryId == id, cancellationToken);

                return Result<TrashPreviewResponse>.Success(new TrashPreviewResponse(
                    e.Id, type, actionName, e.CreatedAtUtc, e.DeletedAtUtc!.Value,
                    [
                        new("Action", actionName),
                        new("OccurredAt", e.OccurredAtUtc.ToString("u")),
                        new("Notes", e.Notes),
                        new("FieldValues", fieldValueCount.ToString())
                    ]));
            }
            case "ActionField":
            {
                var f = await context.ActionFields
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted
                        && context.TrackedActions.Any(a => a.Id == x.TrackedActionId && a.UserId == userId), cancellationToken);

                if (f is null)
                    return Result<TrashPreviewResponse>.Failure("Item not found in trash.", ResultErrorType.NotFound);

                var actionName = await context.TrackedActions
                    .Where(a => a.Id == f.TrackedActionId)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "";

                return Result<TrashPreviewResponse>.Success(new TrashPreviewResponse(
                    f.Id, type, f.Name, f.CreatedAtUtc, f.DeletedAtUtc!.Value,
                    [
                        new("Action", actionName),
                        new("Description", f.Description),
                        new("Unit", f.Unit),
                        new("Order", f.Order.ToString()),
                        new("Required", f.IsRequired.ToString())
                    ]));
            }
            default:
                return Result<TrashPreviewResponse>.Failure($"Unknown trash item type '{type}'.", ResultErrorType.Validation);
        }
    }

    public async Task<Result<int>> PermanentlyDeleteManyAsync(
        IReadOnlyList<TrashItemRef> items, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var actionIds = new List<Guid>();
        var fieldIds = new List<Guid>();
        var entryIds = new List<Guid>();
        var fieldDefIds = new List<Guid>();
        var tagIds = new List<Guid>();

        foreach (var item in items)
        {
            switch (item.Type)
            {
                case "TrackedAction": actionIds.Add(item.Id); break;
                case "ActionField": fieldIds.Add(item.Id); break;
                case "ActionEntry": entryIds.Add(item.Id); break;
                case "FieldDefinition": fieldDefIds.Add(item.Id); break;
                case "Tag": tagIds.Add(item.Id); break;
                default:
                    return Result<int>.Failure($"Unknown trash item type '{item.Type}'.", ResultErrorType.Validation);
            }
        }

        // Ownership + IsDeleted filtering — only touch rows that belong to the caller and are actually in trash.
        actionIds = await context.TrackedActions.IgnoreQueryFilters()
            .Where(a => actionIds.Contains(a.Id) && a.UserId == userId && a.IsDeleted)
            .Select(a => a.Id).ToListAsync(cancellationToken);

        fieldIds = await context.ActionFields.IgnoreQueryFilters()
            .Where(f => fieldIds.Contains(f.Id) && f.IsDeleted
                && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == f.TrackedActionId && a.UserId == userId))
            .Select(f => f.Id).ToListAsync(cancellationToken);

        entryIds = await context.ActionEntries.IgnoreQueryFilters()
            .Where(e => entryIds.Contains(e.Id) && e.IsDeleted
                && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == e.TrackedActionId && a.UserId == userId))
            .Select(e => e.Id).ToListAsync(cancellationToken);

        fieldDefIds = await context.FieldDefinitions.IgnoreQueryFilters()
            .Where(fd => fieldDefIds.Contains(fd.Id) && fd.UserId == userId && fd.IsDeleted)
            .Select(fd => fd.Id).ToListAsync(cancellationToken);

        tagIds = await context.Tags.IgnoreQueryFilters()
            .Where(t => tagIds.Contains(t.Id) && t.UserId == userId && t.IsDeleted)
            .Select(t => t.Id).ToListAsync(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var purged = await TrashPurgeHelper.ExecutePurgeAsync(
                context, userId, actionIds, fieldIds, entryIds, fieldDefIds, tagIds, cancellationToken);

            // If a FieldDefinition was requested but blocked by active refs, surface it.
            if (fieldDefIds.Count > 0)
            {
                var stillThere = await context.FieldDefinitions.IgnoreQueryFilters()
                    .Where(fd => fieldDefIds.Contains(fd.Id))
                    .Select(fd => fd.DefaultName)
                    .ToListAsync(cancellationToken);

                if (stillThere.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<int>.Failure(
                        $"Cannot permanently delete field(s) {string.Join(", ", stillThere)}: still referenced by one or more active actions.",
                        ResultErrorType.Conflict);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(purged);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<int>> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var actionIds = await context.TrackedActions.IgnoreQueryFilters()
            .Where(a => a.UserId == userId && a.IsDeleted)
            .Select(a => a.Id).ToListAsync(cancellationToken);

        var fieldIds = await context.ActionFields.IgnoreQueryFilters()
            .Where(f => f.IsDeleted
                && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == f.TrackedActionId && a.UserId == userId))
            .Select(f => f.Id).ToListAsync(cancellationToken);

        var entryIds = await context.ActionEntries.IgnoreQueryFilters()
            .Where(e => e.IsDeleted
                && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == e.TrackedActionId && a.UserId == userId))
            .Select(e => e.Id).ToListAsync(cancellationToken);

        var fieldDefIds = await context.FieldDefinitions.IgnoreQueryFilters()
            .Where(fd => fd.UserId == userId && fd.IsDeleted)
            .Select(fd => fd.Id).ToListAsync(cancellationToken);

        var tagIds = await context.Tags.IgnoreQueryFilters()
            .Where(t => t.UserId == userId && t.IsDeleted)
            .Select(t => t.Id).ToListAsync(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var purged = await TrashPurgeHelper.ExecutePurgeAsync(
                context, userId, actionIds, fieldIds, entryIds, fieldDefIds, tagIds, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(purged);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
