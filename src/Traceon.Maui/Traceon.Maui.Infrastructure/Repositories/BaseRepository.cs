using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity, TModel>(TraceonDbContext context)
    : IBaseRepository<TEntity, TModel>
    where TEntity : class, IEntityWithId
    where TModel : class, IEntityWithId
{

    #region Protected Members

    protected TraceonDbContext Context = context;
    protected DbSet<TEntity> DbSet = context.Set<TEntity>();

    #endregion Protected Members

    #region Public Methods

    public virtual async Task<Result<IEnumerable<TModel>>> GetAllAsync(bool asNoTracking)
    {
        IQueryable<TEntity>? query = DbSet.AsSplitQuery();

        if (asNoTracking)
            query = query.AsNoTracking();

        var result = query
            .Select(GetProjectExpression());

        return await result.ToListAsync();
    }

    public virtual async Task<Result<TModel>> GetByIdAsync(Guid id, bool asNoTracking)
    {
        IQueryable<TEntity>? query = DbSet.AsSplitQuery()
            .Where(a => a.Id == id);

        if (asNoTracking)
            query = query.AsNoTracking();

        var model = await query
            .Select(GetProjectExpression())
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (model is null)
            return NotFoundError(id);

        return model;
    }

    public virtual async Task<Result> CreateAsync(TModel model)
    {
        var entity = this.MapModelToEntity(model);

        await DbSet.AddAsync(entity);

        OnAfterAddEntity(model, entity);

        return Result.Success();
    }

    public virtual async Task<Result> UpdateAsync(TModel model)
    {
        var query = this.IncludeNavigationProperties(DbSet, false);
        var existingEntity = await query.FirstOrDefaultAsync(e => e.Id == model.Id);

        if (existingEntity == null)
            return NotFoundError(model.Id);

        var modifiedEntity = this.MapModelToEntity(model);

        DbSet.Entry(existingEntity).CurrentValues.SetValues(modifiedEntity);

        OnAfterUpdateValuesInEntity(model, existingEntity);

        return Result.Success();
    }

    public virtual async Task<Result> DeleteAsync(Guid id)
    {
        var query = this.IncludeNavigationProperties(DbSet, false);
        var entity = await query.FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFoundError(id);

        if (entity != null)
            DbSet.Remove(entity);

        return Result.Success();
    }

    #endregion Public Methods

    #region Protected Methods

    protected abstract Expression<Func<TEntity, TModel>> GetProjectExpression();
    protected abstract TEntity MapModelToEntity(TModel model);

    protected virtual IQueryable<TEntity> IncludeNavigationProperties(DbSet<TEntity> dbSet, bool asNoTracking)
    {
        // By default, no includes. Override in derived classes if and when needed.
        if (asNoTracking)
            return dbSet.AsNoTracking().AsSplitQuery();
        else
            return dbSet.AsSplitQuery();
    }

    protected virtual void OnAfterAddEntity(TModel model, TEntity createdEntity)
    {
        return;
    }

    protected virtual void OnAfterUpdateValuesInEntity(TModel model, TEntity updatedEntity)
    {
        return;
    }

    protected virtual ResultNotFoundError NotFoundError(Guid id)
        => new($"{typeof(TEntity).Name} with Id '{id}' not found.");

    #endregion Protected Methods
}