using System.Net.Http.Json;
using Traceon.Contracts.ConnectedActionRules;

namespace Traceon.Blazor.Services;

public sealed class ConnectedActionRuleService(HttpClient http)
{
    public async Task<List<ConnectedActionRuleResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var rules = await http.GetFromJsonAsync<List<ConnectedActionRuleResponse>>(
            $"/api/tracked-actions/{trackedActionId}/connected-action-rules") ?? [];
        return rules.OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateConnectedActionRuleRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/connected-action-rules", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid ruleId, UpdateConnectedActionRuleRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/connected-action-rules/{ruleId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid ruleId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/connected-action-rules/{ruleId}");
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
