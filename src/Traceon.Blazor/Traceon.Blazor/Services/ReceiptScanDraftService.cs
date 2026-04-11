using System.Net.Http.Json;
using Traceon.Contracts.ReceiptScanDraft;

namespace Traceon.Blazor.Services;

public sealed class ReceiptScanDraftService(HttpClient http)
{
    private const string BaseUrl = "/api/receipt-scan-drafts";

    public async Task<List<ReceiptScanDraftResponse>> GetMyDraftsAsync()
    {
        return await http.GetFromJsonAsync<List<ReceiptScanDraftResponse>>(BaseUrl) ?? [];
    }

    public async Task<ReceiptScanDraftResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            return await http.GetFromJsonAsync<ReceiptScanDraftResponse>($"{BaseUrl}/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ReceiptScanDraftResponse?> CreateAsync(CreateReceiptScanDraftRequest request)
    {
        var response = await http.PostAsJsonAsync(BaseUrl, request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ReceiptScanDraftResponse>();
        return null;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateReceiptScanDraftRequest request)
    {
        var response = await http.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
