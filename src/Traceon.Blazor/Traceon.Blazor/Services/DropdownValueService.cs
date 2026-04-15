using System.Net.Http.Json;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Blazor.Services;

public sealed class DropdownValueService(HttpClient http)
{
    public async Task<List<DropdownValueResponse>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId)
    {
        return await http.GetFromJsonAsync<List<DropdownValueResponse>>(
            $"/api/dropdown-values/by-field/{fieldDefinitionId}") ?? [];
    }

    public async Task<(bool Success, DropdownValueResponse? Renamed, IReadOnlyList<string> Errors)> RenameAsync(Guid id, string newValue)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/dropdown-values/{id}/rename",
            new RenameDropdownValueRequest(newValue));

        if (response.IsSuccessStatusCode)
        {
            var renamed = await response.Content.ReadFromJsonAsync<DropdownValueResponse>();
            return (true, renamed, []);
        }

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, null, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ReorderAsync(Guid fieldDefinitionId, List<Guid> orderedIds)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/dropdown-values/by-field/{fieldDefinitionId}/reorder",
            new ReorderDropdownValuesRequest(orderedIds));

        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/dropdown-values/{id}");
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> SyncAsync(Guid fieldDefinitionId)
    {
        var response = await http.PostAsync(
            $"/api/dropdown-values/by-field/{fieldDefinitionId}/sync", null);

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
