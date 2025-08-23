using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class BaseRepository<TEntity> 
    : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly TraceonDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(TraceonDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await _dbSet.AsNoTracking().ToListAsync();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public virtual async Task CreateAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }
}