using Traceon.Application.Common;
using Traceon.Contracts.Tags;

namespace Traceon.Application.Services;

public interface ITagService
{
    IQueryable<TagResponse> QueryAll();
    Task<Result<TagResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TagResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TagResponse>> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default);
    Task<Result<TagResponse>> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}
