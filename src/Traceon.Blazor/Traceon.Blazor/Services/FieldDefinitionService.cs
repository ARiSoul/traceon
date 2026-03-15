using System.Net.Http.Json;
using Traceon.Blazor.Components;
using Traceon.Contracts.FieldDefinitions;

namespace Traceon.Blazor.Services;

public sealed class FieldDefinitionService(HttpClient http)
{
    public async Task<DataGridResult<FieldDefinitionResponse>> QueryAsync(DataGridRequest request, string[]? searchFields = null)
    {
        var queryString = ODataQueryBuilder.BuildQueryString(request, searchFields);
        using var response = await http.GetAsync($"/api/field-definitions?{queryString}");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<FieldDefinitionResponse>>() ?? [];
        var totalCount = GetTotalCount(response, items.Count);

        return new DataGridResult<FieldDefinitionResponse>(items, totalCount);
    }

    public async Task<FieldDefinitionResponse?> GetByIdAsync(Guid id)
    {
        return await http.GetFromJsonAsync<FieldDefinitionResponse>($"/api/field-definitions/{id}");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(CreateFieldDefinitionRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/field-definitions", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(Guid id, UpdateFieldDefinitionRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/field-definitions/{id}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/field-definitions/{id}");
        return await ToResultAsync(response);
    }

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }

    private static int GetTotalCount(HttpResponseMessage response, int fallbackCount)
    {
        if (response.Headers.TryGetValues("X-Total-Count", out var values)
            && int.TryParse(values.FirstOrDefault(), out var count))
            return count;

        return fallbackCount;
    }
}
