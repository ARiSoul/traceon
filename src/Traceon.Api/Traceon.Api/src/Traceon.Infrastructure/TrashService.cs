using Microsoft.EntityFrameworkCore;
using Traceon.Application.Interfaces;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure;

public sealed record DeletedItemResponse(Guid Id, string Type, string Name, DateTime DeletedAtUtc);

public sealed class TrashService(TraceonDbContext context, ICurrentUserService currentUser)
{
    public async Task<List<DeletedItemResponse>> GetDeletedItemsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var actions = await context.TrackedActions
            .IgnoreQueryFilters()
            .Where(a => a.UserId == userId && a.IsDeleted && a.DeletedAtUtc != null)
            .Select(a => new DeletedItemResponse(a.Id, "TrackedAction", a.Name, a.DeletedAtUtc!.Value))
            .ToListAsync(cancellationToken);

        var fieldDefs = await context.FieldDefinitions
            .IgnoreQueryFilters()
            .Where(fd => fd.UserId == userId && fd.IsDeleted && fd.DeletedAtUtc != null)
            .Select(fd => new DeletedItemResponse(fd.Id, "FieldDefinition", fd.DefaultName, fd.DeletedAtUtc!.Value))
            .ToListAsync(cancellationToken);

        var tags = await context.Tags
            .IgnoreQueryFilters()
            .Where(t => t.UserId == userId && t.IsDeleted && t.DeletedAtUtc != null)
            .Select(t => new DeletedItemResponse(t.Id, "Tag", t.Name, t.DeletedAtUtc!.Value))
            .ToListAsync(cancellationToken);

        return [.. actions, .. fieldDefs, .. tags];
    }
}
