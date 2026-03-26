using System.Net.Http.Json;
using Traceon.Blazor.Services;

namespace Traceon.Blazor.Auth;

public sealed class AuthService(HttpClient http, TokenStore tokenStore, TokenAuthStateProvider authState)
{
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

        tokenStore.SetSession(token.AccessToken, token.ExpiresIn, request.Email);
        await tokenStore.PersistRefreshTokenAsync(token.RefreshToken, request.Email);

        authState.NotifyUserAuthentication(request.Email);

        return (true, []);
    }

    public async Task LogoutAsync()
    {
        tokenStore.Clear();
        await tokenStore.ClearPersistedTokensAsync();
        authState.NotifyUserLogout();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/forgot-password", request);

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/reset-password", request);

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var response = await http.PostAsJsonAsync("/api/identity/change-password",
            new { CurrentPassword = currentPassword, NewPassword = newPassword });

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAccountAsync()
    {
        var response = await http.DeleteAsync("/api/identity/account");

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    private static Task<IReadOnlyList<string>> ExtractErrorsAsync(HttpResponseMessage response)
        => ApiErrorParser.ExtractErrorsAsync(response);
}
