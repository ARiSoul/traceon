using System.Net.Http.Json;
using Traceon.Contracts.FieldAnalyticsRules;

namespace Traceon.Blazor.Services;

public sealed class FieldAnalyticsRuleService(HttpClient http)
{
    public async Task<List<FieldAnalyticsRuleResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var rules = await http.GetFromJsonAsync<List<FieldAnalyticsRuleResponse>>(
            $"/api/tracked-actions/{trackedActionId}/analytics-rules") ?? [];
        return rules.OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateFieldAnalyticsRuleRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/analytics-rules", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid ruleId, UpdateFieldAnalyticsRuleRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/analytics-rules/{ruleId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid ruleId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/analytics-rules/{ruleId}");
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
