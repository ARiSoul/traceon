using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity, TModel>(TraceonDbContext context, MapperlyConfiguration mapper)
    : IBaseRepository<TEntity, TModel>
    where TEntity : class
    where TModel : class
{
    protected TraceonDbContext Context = context;
    protected DbSet<TEntity> DbSet = context.Set<TEntity>();
    protected MapperlyConfiguration Mapper = mapper;

    public abstract IEnumerable<TModel> MapEntityToModelCollection(IEnumerable<TEntity> entities);
    public abstract TModel MapEntityToModel(TEntity entity);
    public abstract TEntity MapModelToEntity(TModel model);

    public virtual async Task<Result<IEnumerable<TModel>>> GetAllAsync()
    {
        var entities = await DbSet.AsNoTracking().ToListAsync();

        return Result.Success(this.MapEntityToModelCollection(entities));
    }

    public virtual async Task<Result<TModel>> GetByIdAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);

        if (entity == null)
            return new ResultNotFoundError($"{typeof(TEntity).Name} with Id '{id}' not found.");

        return this.MapEntityToModel(entity);
    }

    public virtual async Task<Result> CreateAsync(TModel model)
    {
        var entity = this.MapModelToEntity(model);

        await DbSet.AddAsync(entity);

        return Result.Success();
    }

    public virtual Task<Result> UpdateAsync(TModel model)
    {
        var entity = this.MapModelToEntity(model);

        DbSet.Update(entity);
        
        return Task.FromResult(Result.Success());
    }

    public virtual async Task<Result> DeleteAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);

        if (entity == null)
            return new ResultNotFoundError($"{typeof(TEntity).Name} with Id '{id}' not found.");

        if (entity != null)
            DbSet.Remove(entity);

        return Result.Success();
    }
}