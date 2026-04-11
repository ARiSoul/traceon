using System.Net.Http.Json;
using Traceon.Contracts.ReceiptImport;

namespace Traceon.Blazor.Services;

public sealed class ReceiptImportConfigService(HttpClient http)
{
    public async Task<ReceiptImportConfigResponse?> GetByActionAsync(Guid trackedActionId)
    {
        try
        {
            return await http.GetFromJsonAsync<ReceiptImportConfigResponse>(
                $"/api/tracked-actions/{trackedActionId}/receipt-config");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpsertAsync(
        Guid trackedActionId, UpdateReceiptImportConfigRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/receipt-config", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid trackedActionId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/receipt-config");
        return await ToResultAsync(response);
    }

    public async Task<List<ReceiptMappingRuleResponse>> GetMappingRulesAsync(Guid trackedActionId)
    {
        return await http.GetFromJsonAsync<List<ReceiptMappingRuleResponse>>(
            $"/api/tracked-actions/{trackedActionId}/receipt-config/mapping-rules") ?? [];
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateMappingRuleAsync(
        Guid trackedActionId, CreateReceiptMappingRuleRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/receipt-config/mapping-rules", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateMappingRuleAsync(
        Guid trackedActionId, Guid ruleId, UpdateReceiptMappingRuleRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/receipt-config/mapping-rules/{ruleId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteMappingRuleAsync(
        Guid trackedActionId, Guid ruleId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/receipt-config/mapping-rules/{ruleId}");
        return await ToResultAsync(response);
    }

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }
}
