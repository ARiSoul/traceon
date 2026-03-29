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

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return (false, ["EmailNotConfirmed"]);
        }

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

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/confirm-email", request);

        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ExtractErrorsAsync(response);
        return (false, errors);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ResendConfirmationAsync(ResendConfirmationRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/identity/resend-confirmation", request);

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

    public async Task<IReadOnlyList<string>> GetExternalProvidersAsync()
    {
        try
        {
            var providers = await http.GetFromJsonAsync<List<string>>("/api/identity/external-providers");
            return providers ?? [];
        }
        catch
        {
            return [];
        }
    }

    public string GetExternalLoginUrl(string provider) =>
        $"{http.BaseAddress}api/identity/external-login?provider={Uri.EscapeDataString(provider)}";

    private static Task<IReadOnlyList<string>> ExtractErrorsAsync(HttpResponseMessage response)
        => ApiErrorParser.ExtractErrorsAsync(response);
}
