using Traceon.Application.Common;
using Traceon.Contracts.EntryTemplates;

namespace Traceon.Application.Services;

public interface IEntryTemplateService
{
    Task<Result<IReadOnlyList<EntryTemplateResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<EntryTemplateResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<EntryTemplateResponse>> CreateAsync(Guid trackedActionId, CreateEntryTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<EntryTemplateResponse>> UpdateAsync(Guid id, UpdateEntryTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}
