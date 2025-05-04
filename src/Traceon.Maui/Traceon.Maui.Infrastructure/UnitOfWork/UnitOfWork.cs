using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TraceonDbContext _context;

    public IFieldDefinitionRepository FieldDefinitions { get; }
    public ITagRepository Tags { get; }
    public ITrackedActionRepository TrackedActions { get; }

    public UnitOfWork(
        TraceonDbContext context,
        IFieldDefinitionRepository fieldDefinitions,
        ITagRepository tags,
        ITrackedActionRepository trackedActions)
    {
        _context = context;
        FieldDefinitions = fieldDefinitions;
        Tags = tags;
        TrackedActions = trackedActions;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
