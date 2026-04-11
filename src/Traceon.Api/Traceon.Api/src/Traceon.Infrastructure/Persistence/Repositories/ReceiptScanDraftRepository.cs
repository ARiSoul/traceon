using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ReceiptScanDraftRepository(TraceonDbContext context) : IReceiptScanDraftRepository
{
    public async Task<IReadOnlyList<ReceiptScanDraft>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptScanDrafts
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAtUtc ?? d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReceiptScanDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptScanDrafts.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(ReceiptScanDraft entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptScanDrafts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ReceiptScanDraft entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptScanDrafts.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ReceiptScanDrafts
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
