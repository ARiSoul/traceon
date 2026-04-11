using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Mapping;
using Traceon.Contracts.ReceiptImport;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ReceiptImportConfigService(
    IReceiptImportConfigRepository repository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
    ICurrentUserService currentUser,
    ILogger<ReceiptImportConfigService> logger) : IReceiptImportConfigService
{
    public async Task<Result<ReceiptImportConfigResponse>> GetByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ReceiptImportConfigResponse>.Failure("Tracked action not found.");

        var config = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        if (config is null)
            return Result<ReceiptImportConfigResponse>.Failure("Receipt import config not found.", ResultErrorType.NotFound);

        return Result<ReceiptImportConfigResponse>.Success(config.ToResponse());
    }

    public async Task<Result<ReceiptImportConfigResponse>> UpsertAsync(
        Guid trackedActionId, UpdateReceiptImportConfigRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ReceiptImportConfigResponse>.Failure("Tracked action not found.");

        var config = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);

        if (config is null)
        {
            config = ReceiptImportConfig.Create(trackedActionId);
            config.Update(
                request.ShopFieldId,
                request.DescriptionFieldId,
                request.TotalFieldId,
                request.QuantityFieldId,
                request.UnitPriceFieldId,
                request.StaticFieldValues);
            await repository.AddAsync(config, cancellationToken);
            logger.LogInformation("Receipt import config created for action {ActionId}.", trackedActionId);
        }
        else
        {
            config.Update(
                request.ShopFieldId,
                request.DescriptionFieldId,
                request.TotalFieldId,
                request.QuantityFieldId,
                request.UnitPriceFieldId,
                request.StaticFieldValues);
            await repository.UpdateAsync(config, cancellationToken);
            logger.LogInformation("Receipt import config updated for action {ActionId}.", trackedActionId);
        }

        return Result<ReceiptImportConfigResponse>.Success(config.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result.Failure("Tracked action not found.");

        var config = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        if (config is null)
            return Result.Failure("Receipt import config not found.");

        await repository.DeleteAsync(config.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ReceiptMappingRuleResponse>>> GetMappingRulesAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<IReadOnlyList<ReceiptMappingRuleResponse>>.Failure("Tracked action not found.");

        var config = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        if (config is null)
            return Result<IReadOnlyList<ReceiptMappingRuleResponse>>.Success(Array.Empty<ReceiptMappingRuleResponse>());

        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        var rules = await repository.GetMappingRulesByConfigIdAsync(config.Id, cancellationToken);
        var responses = rules
            .Select(r => r.ToResponse(fieldNames.GetValueOrDefault(r.TargetFieldId, "?")))
            .ToList();

        return Result<IReadOnlyList<ReceiptMappingRuleResponse>>.Success(responses);
    }

    public async Task<Result<ReceiptMappingRuleResponse>> CreateMappingRuleAsync(
        Guid trackedActionId, CreateReceiptMappingRuleRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result<ReceiptMappingRuleResponse>.Failure("Tracked action not found.");

        var config = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        if (config is null)
        {
            config = ReceiptImportConfig.Create(trackedActionId);
            await repository.AddAsync(config, cancellationToken);
        }

        var entity = ReceiptMappingRule.Create(
            config.Id,
            request.TargetFieldId,
            request.Pattern,
            request.Value,
            request.Priority);

        await repository.AddMappingRuleAsync(entity, cancellationToken);

        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.LogInformation("Receipt mapping rule {RuleId} created for action {ActionId}.", entity.Id, trackedActionId);
        return Result<ReceiptMappingRuleResponse>.Success(
            entity.ToResponse(fieldNames.GetValueOrDefault(entity.TargetFieldId, "?")));
    }

    public async Task<Result<ReceiptMappingRuleResponse>> UpdateMappingRuleAsync(
        Guid ruleId, UpdateReceiptMappingRuleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetMappingRuleByIdAsync(ruleId, cancellationToken);
        if (entity is null)
            return Result<ReceiptMappingRuleResponse>.Failure("Mapping rule not found.");

        entity.Update(request.Pattern, request.Value, request.Priority);
        await repository.UpdateMappingRuleAsync(entity, cancellationToken);

        return Result<ReceiptMappingRuleResponse>.Success(entity.ToResponse("?"));
    }

    public async Task<Result> DeleteMappingRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetMappingRuleByIdAsync(ruleId, cancellationToken);
        if (entity is null)
            return Result.Failure("Mapping rule not found.");

        await repository.DeleteMappingRuleAsync(ruleId, cancellationToken);
        return Result.Success();
    }
}
