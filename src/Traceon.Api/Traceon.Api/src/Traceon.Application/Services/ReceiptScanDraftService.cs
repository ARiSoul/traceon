using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Mapping;
using Traceon.Contracts.ReceiptScanDraft;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ReceiptScanDraftService(
    IReceiptScanDraftRepository repository,
    ICurrentUserService currentUser,
    ILogger<ReceiptScanDraftService> logger) : IReceiptScanDraftService
{
    public async Task<Result<IReadOnlyList<ReceiptScanDraftResponse>>> GetMyDraftsAsync(
        CancellationToken cancellationToken = default)
    {
        var drafts = await repository.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        var responses = drafts.Select(d => d.ToResponse()).ToList() as IReadOnlyList<ReceiptScanDraftResponse>;
        return Result<IReadOnlyList<ReceiptScanDraftResponse>>.Success(responses);
    }

    public async Task<Result<ReceiptScanDraftResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var draft = await repository.GetByIdAsync(id, cancellationToken);
        if (draft is null || draft.UserId != currentUser.UserId)
            return Result<ReceiptScanDraftResponse>.Failure("Draft not found.", ResultErrorType.NotFound);

        return Result<ReceiptScanDraftResponse>.Success(draft.ToResponse());
    }

    public async Task<Result<ReceiptScanDraftResponse>> CreateAsync(
        CreateReceiptScanDraftRequest request, CancellationToken cancellationToken = default)
    {
        var draft = ReceiptScanDraft.Create(
            currentUser.UserId,
            request.SelectedActionId,
            request.SelectedActionName,
            request.SerializedState,
            request.MerchantName,
            request.TransactionDate,
            request.Total,
            request.CurrentStep);

        await repository.AddAsync(draft, cancellationToken);
        logger.LogInformation("Created receipt scan draft {DraftId} for user {UserId}.", draft.Id, currentUser.UserId);

        return Result<ReceiptScanDraftResponse>.Success(draft.ToResponse());
    }

    public async Task<Result<ReceiptScanDraftResponse>> UpdateAsync(
        Guid id, UpdateReceiptScanDraftRequest request, CancellationToken cancellationToken = default)
    {
        var draft = await repository.GetByIdAsync(id, cancellationToken);
        if (draft is null || draft.UserId != currentUser.UserId)
            return Result<ReceiptScanDraftResponse>.Failure("Draft not found.", ResultErrorType.NotFound);

        draft.Update(
            request.SerializedState,
            request.MerchantName,
            request.TransactionDate,
            request.Total,
            request.CurrentStep);

        await repository.UpdateAsync(draft, cancellationToken);

        return Result<ReceiptScanDraftResponse>.Success(draft.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var draft = await repository.GetByIdAsync(id, cancellationToken);
        if (draft is null || draft.UserId != currentUser.UserId)
            return Result.Failure("Draft not found.", ResultErrorType.NotFound);

        await repository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Deleted receipt scan draft {DraftId} for user {UserId}.", id, currentUser.UserId);

        return Result.Success();
    }
}
