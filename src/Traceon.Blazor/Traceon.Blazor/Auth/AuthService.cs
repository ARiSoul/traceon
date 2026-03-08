using System.Net.Http.Json;
using Blazored.LocalStorage;
using Traceon.Blazor.Services;

namespace Traceon.Blazor.Auth;

public sealed class AuthService(HttpClient http, ILocalStorageService localStorage, TokenAuthStateProvider authState)
{
    private const string TokenKey = "traceon_access_token";
    private const string RefreshTokenKey = "traceon_refresh_token";

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RegisterAsync(RegisterRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/register", request);

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> LoginAsync(LoginRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ExtractErrorsAsync(response);
            return (false, errors);
        }

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        if (token is null)
            return (false, ["Failed to parse login response."]);

        await localStorage.SetItemAsStringAsync(TokenKey, token.AccessToken);
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, token.RefreshToken);

        authState.NotifyUserAuthentication(token.AccessToken);

        return (true, []);
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync(TokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        authState.NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await localStorage.GetItemAsStringAsync(TokenKey);
    }

    private static Task<IReadOnlyList<string>> ExtractErrorsAsync(HttpResponseMessage response)
        => ApiErrorParser.ExtractErrorsAsync(response);
}
