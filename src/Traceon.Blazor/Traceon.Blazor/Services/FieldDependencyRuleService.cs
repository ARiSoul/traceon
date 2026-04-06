using System.Net.Http.Json;
using Traceon.Contracts.FieldDependencyRules;

namespace Traceon.Blazor.Services;

public sealed class FieldDependencyRuleService(HttpClient http)
{
    public async Task<List<FieldDependencyRuleResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var rules = await http.GetFromJsonAsync<List<FieldDependencyRuleResponse>>(
            $"/api/tracked-actions/{trackedActionId}/dependency-rules") ?? [];
        return rules.OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateFieldDependencyRuleRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/dependency-rules", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid ruleId, UpdateFieldDependencyRuleRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/dependency-rules/{ruleId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid ruleId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/dependency-rules/{ruleId}");
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
