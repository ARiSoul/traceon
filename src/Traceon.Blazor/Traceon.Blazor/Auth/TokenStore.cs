using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace Traceon.Blazor.Auth;

public sealed class TokenStore(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory)
{
    private const string RefreshTokenKey = "traceon_refresh_token";
    private const string EmailKey = "traceon_user_email";

    private string? _accessToken;
    private DateTimeOffset _expiresAt;
    private string? _email;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public string? Email => _email;

    public void SetSession(string accessToken, int expiresInSeconds, string email)
    {
        _accessToken = accessToken;
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(expiresInSeconds - 60, 0));
        _email = email;
    }

    public void Clear()
    {
        _accessToken = null;
        _expiresAt = default;
        _email = null;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
            return _accessToken;

        return await TryRefreshAsync();
    }

    public async Task<string?> ForceRefreshAsync()
    {
        return await TryRefreshAsync(force: true);
    }

    public async Task PersistRefreshTokenAsync(string refreshToken, string email)
    {
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, refreshToken);
        await localStorage.SetItemAsStringAsync(EmailKey, email);
    }

    public async Task ClearPersistedTokensAsync()
    {
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(EmailKey);
    }

    public async Task<string?> GetPersistedEmailAsync()
    {
        return await localStorage.GetItemAsStringAsync(EmailKey);
    }

    private async Task<string?> TryRefreshAsync(bool force = false)
    {
        await _refreshLock.WaitAsync();
        try
        {
            if (!force && _accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
                return _accessToken;

            var refreshToken = await localStorage.GetItemAsStringAsync(RefreshTokenKey);
            var email = await localStorage.GetItemAsStringAsync(EmailKey);
            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(email))
            {
                Clear();
                return null;
            }

            var client = httpClientFactory.CreateClient("TraceonApiAnonymous");
            var response = await client.PostAsJsonAsync("/api/identity/refresh", new RefreshRequest(email, refreshToken));

            if (!response.IsSuccessStatusCode)
            {
                Clear();
                await ClearPersistedTokensAsync();
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
            if (token is null)
            {
                Clear();
                await ClearPersistedTokensAsync();
                return null;
            }

            _accessToken = token.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn - 60, 0));
            _email = email;

            await localStorage.SetItemAsStringAsync(RefreshTokenKey, token.RefreshToken);

            return _accessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}
