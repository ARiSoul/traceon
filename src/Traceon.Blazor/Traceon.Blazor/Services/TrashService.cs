using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed record DeletedItemResponse(
    Guid Id,
    string Type,
    string Name,
    DateTime DeletedAtUtc,
    Guid? ParentId = null);

public sealed record TrashItemRef(string Type, Guid Id);

public sealed record TrashPreviewDetail(string Key, string? Value);

public sealed record TrashPreviewResponse(
    Guid Id,
    string Type,
    string Name,
    DateTime CreatedAtUtc,
    DateTime DeletedAtUtc,
    List<TrashPreviewDetail> Details);

public sealed class TrashService(HttpClient http)
{
    public async Task<List<DeletedItemResponse>> GetDeletedItemsAsync()
    {
        return await http.GetFromJsonAsync<List<DeletedItemResponse>>("/api/trash") ?? [];
    }

    public async Task<(TrashPreviewResponse? Preview, IReadOnlyList<string> Errors)> GetPreviewAsync(string type, Guid id)
    {
        var response = await http.GetAsync($"/api/trash/{type}/{id}/preview");
        if (!response.IsSuccessStatusCode)
            return (null, await ApiErrorParser.ExtractErrorsAsync(response));

        var preview = await response.Content.ReadFromJsonAsync<TrashPreviewResponse>();
        return (preview, []);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> PermanentlyDeleteAsync(IReadOnlyList<TrashItemRef> items)
    {
        var response = await http.PostAsJsonAsync("/api/trash/permanent-delete", items);
        if (response.IsSuccessStatusCode)
            return (true, []);
        return (false, await ApiErrorParser.ExtractErrorsAsync(response));
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ClearAllAsync()
    {
        var response = await http.DeleteAsync("/api/trash");
        if (response.IsSuccessStatusCode)
            return (true, []);
        return (false, await ApiErrorParser.ExtractErrorsAsync(response));
    }
}
