using Traceon.Application.Common;
using Traceon.Contracts.Analytics;

namespace Traceon.Application.Services;

public interface IActionChartVisibilityService
{
    Task<Result<ChartVisibilityResponse>> GetAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ChartVisibilityResponse>> UpsertAsync(Guid trackedActionId, UpdateChartVisibilityRequest request, CancellationToken cancellationToken = default);
}
