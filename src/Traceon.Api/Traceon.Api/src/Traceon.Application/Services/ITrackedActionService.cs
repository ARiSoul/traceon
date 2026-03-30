using Traceon.Application.Common;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Application.Services;

public interface ITrackedActionService
{
    IQueryable<TrackedActionResponse> QueryAll();
    Task<Result<TrackedActionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TrackedActionResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TrackedActionResponse>> CreateAsync(CreateTrackedActionRequest request, CancellationToken cancellationToken = default);
    Task<Result<TrackedActionResponse>> UpdateAsync(Guid id, UpdateTrackedActionRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Traceon.Contracts.Tags.TagResponse>>> GetTagsAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result> AddTagAsync(Guid trackedActionId, Guid tagId, CancellationToken cancellationToken = default);
    Task<Result> RemoveTagAsync(Guid trackedActionId, Guid tagId, CancellationToken cancellationToken = default);
    Task<Result> ReorderAsync(List<Guid> orderedIds, CancellationToken cancellationToken = default);
}
