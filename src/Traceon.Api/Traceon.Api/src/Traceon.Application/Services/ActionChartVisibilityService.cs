using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.Analytics;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ActionChartVisibilityService(
    IActionChartVisibilityRepository repository,
    ITrackedActionRepository actionRepository,
    ICurrentUserService currentUser) : IActionChartVisibilityService
{
    private const char Separator = '|';

    public async Task<Result<ChartVisibilityResponse>> GetAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ChartVisibilityResponse>.Failure("Tracked action not found.");

        var entity = await repository.GetByActionAndUserAsync(trackedActionId, currentUser.UserId!, cancellationToken);
        return Result<ChartVisibilityResponse>.Success(new ChartVisibilityResponse(
            Split(entity?.HiddenKeys),
            Split(entity?.ChartOrder)));
    }

    public async Task<Result<ChartVisibilityResponse>> UpsertAsync(
        Guid trackedActionId, UpdateChartVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ChartVisibilityResponse>.Failure("Tracked action not found.");

        var sanitized = Sanitize(request.HiddenKeys);
        var serialized = string.Join(Separator, sanitized);

        var entity = await repository.GetByActionAndUserAsync(trackedActionId, currentUser.UserId!, cancellationToken);
        if (entity is null)
        {
            entity = ActionChartVisibility.Create(currentUser.UserId!, trackedActionId, serialized);
            await repository.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.SetHiddenKeys(serialized);
            await repository.UpdateAsync(entity, cancellationToken);
        }

        return Result<ChartVisibilityResponse>.Success(new ChartVisibilityResponse(sanitized, Split(entity.ChartOrder)));
    }

    public async Task<Result<ChartVisibilityResponse>> UpsertOrderAsync(
        Guid trackedActionId, UpdateChartOrderRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ChartVisibilityResponse>.Failure("Tracked action not found.");

        var sanitized = Sanitize(request.ChartOrder);
        var serialized = string.Join(Separator, sanitized);

        var entity = await repository.GetByActionAndUserAsync(trackedActionId, currentUser.UserId!, cancellationToken);
        if (entity is null)
        {
            entity = ActionChartVisibility.Create(currentUser.UserId!, trackedActionId, string.Empty, serialized);
            await repository.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.SetChartOrder(serialized);
            await repository.UpdateAsync(entity, cancellationToken);
        }

        return Result<ChartVisibilityResponse>.Success(new ChartVisibilityResponse(Split(entity.HiddenKeys), sanitized));
    }

    private static List<string> Sanitize(List<string>? keys) =>
        (keys ?? [])
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static List<string> Split(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
