using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Arisoul.Traceon.Maui.Infrastructure.UnitOfWork;

public class UnitOfWork(
    TraceonDbContext context,
    IServiceProvider serviceProvider) : IUnitOfWork
{
    private readonly TraceonDbContext _context = context;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IFieldDefinitionRepository FieldDefinitions => _serviceProvider.GetRequiredService<IFieldDefinitionRepository>();
    public ITagRepository Tags => _serviceProvider.GetRequiredService<ITagRepository>();
    public ITrackedActionRepository TrackedActions => _serviceProvider.GetRequiredService<ITrackedActionRepository>();
    public IActionFieldRepository ActionFields => _serviceProvider.GetRequiredService<IActionFieldRepository>();

    public async Task<int> SaveChangesAsync()
    {
        var affected = await _context.SaveChangesAsync().ConfigureAwait(false);

        return affected;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
