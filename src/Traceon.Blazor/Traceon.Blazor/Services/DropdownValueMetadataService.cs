using System.Net.Http.Json;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Blazor.Services;

public sealed class DropdownValueMetadataService(HttpClient http)
{
    public async Task<List<DropdownValueMetadataFieldResponse>> GetFieldsAsync(Guid fieldDefinitionId)
    {
        return await http.GetFromJsonAsync<List<DropdownValueMetadataFieldResponse>>(
            $"/api/dropdown-values/metadata-fields/by-field/{fieldDefinitionId}") ?? [];
    }

    public async Task<List<DropdownValueMetadataFieldResponse>> GetAllFieldsAsync()
    {
        return await http.GetFromJsonAsync<List<DropdownValueMetadataFieldResponse>>(
            "/api/dropdown-values/metadata-fields/all") ?? [];
    }

    public async Task<(bool Success, DropdownValueMetadataFieldResponse? Created, IReadOnlyList<string> Errors)> CreateFieldAsync(
        CreateDropdownValueMetadataFieldRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/dropdown-values/metadata-fields", request);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<DropdownValueMetadataFieldResponse>();
            return (true, created, []);
        }

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, null, errors);
    }

    public async Task<(bool Success, DropdownValueMetadataFieldResponse? Updated, IReadOnlyList<string> Errors)> UpdateFieldAsync(
        Guid id,
        UpdateDropdownValueMetadataFieldRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/dropdown-values/metadata-fields/{id}", request);

        if (response.IsSuccessStatusCode)
        {
            var updated = await response.Content.ReadFromJsonAsync<DropdownValueMetadataFieldResponse>();
            return (true, updated, []);
        }

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, null, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteFieldAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/dropdown-values/metadata-fields/{id}");
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ReorderFieldsAsync(Guid fieldDefinitionId, List<Guid> orderedIds)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/dropdown-values/metadata-fields/by-field/{fieldDefinitionId}/reorder",
            new ReorderDropdownValueMetadataFieldsRequest(orderedIds));

        return await ToResultAsync(response);
    }

    public async Task<List<DropdownValueMetadataValueEntry>> GetValuesAsync(Guid dropdownValueId)
    {
        return await http.GetFromJsonAsync<List<DropdownValueMetadataValueEntry>>(
            $"/api/dropdown-values/{dropdownValueId}/metadata") ?? [];
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpsertValuesAsync(
        Guid dropdownValueId,
        List<DropdownValueMetadataValueEntry> values)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/dropdown-values/{dropdownValueId}/metadata",
            new UpsertDropdownValueMetadataRequest(values));

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
