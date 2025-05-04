namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IUnitOfWork
{
    IFieldDefinitionRepository FieldDefinitions { get; }
    ITagRepository Tags { get; }
    ITrackedActionRepository TrackedActions { get; }
    Task<int> SaveChangesAsync();
}
