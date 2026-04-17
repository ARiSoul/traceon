using Traceon.Application.Common;
using Traceon.Contracts.Analytics;

namespace Traceon.Application.Services;

public interface IActionChartVisibilityService
{
    Task<Result<ChartVisibilityResponse>> GetAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ChartVisibilityResponse>> UpsertAsync(Guid trackedActionId, UpdateChartVisibilityRequest request, CancellationToken cancellationToken = default);
    Task<Result<ChartVisibilityResponse>> UpsertOrderAsync(Guid trackedActionId, UpdateChartOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>Rewrites chart keys in every user's visibility and order for the given action.</summary>
    Task RenameKeysAsync(Guid trackedActionId, IReadOnlyDictionary<string, string> renames, CancellationToken cancellationToken = default);
}
