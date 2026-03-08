using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class TagRepository(TraceonDbContext context) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tags.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.Tags
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string userId, string name, CancellationToken cancellationToken = default)
    {
        return await context.Tags
            .AnyAsync(t => t.UserId == userId && t.Name == name, cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        context.Tags.Add(tag);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        context.Tags.Update(tag);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.Tags
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
