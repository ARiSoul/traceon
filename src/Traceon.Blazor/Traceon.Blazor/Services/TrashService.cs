using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed record DeletedItemResponse(Guid Id, string Type, string Name, DateTime DeletedAtUtc);

public sealed class TrashService(HttpClient http)
{
    public async Task<List<DeletedItemResponse>> GetDeletedItemsAsync()
    {
        return await http.GetFromJsonAsync<List<DeletedItemResponse>>("/api/trash") ?? [];
    }
}
