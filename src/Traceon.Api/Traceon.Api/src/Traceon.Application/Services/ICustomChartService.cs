using Traceon.Application.Common;
using Traceon.Contracts.CustomCharts;

namespace Traceon.Application.Services;

public interface ICustomChartService
{
    Task<Result<IReadOnlyList<CustomChartResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<CustomChartResponse>> CreateAsync(Guid trackedActionId, CreateCustomChartRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomChartResponse>> UpdateAsync(Guid id, UpdateCustomChartRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
