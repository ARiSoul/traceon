using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Traceon.Blazor.Auth;

public sealed class TokenAuthStateProvider(TokenStore tokenStore) : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser.Identity?.IsAuthenticated != true)
        {
            var accessToken = await tokenStore.GetAccessTokenAsync();

            if (accessToken is not null)
            {
                var email = tokenStore.Email ?? await tokenStore.GetPersistedEmailAsync();

                if (!string.IsNullOrWhiteSpace(email))
                    _currentUser = CreatePrincipal(email);
            }
        }

        return new AuthenticationState(_currentUser);
    }

    public void NotifyUserAuthentication(string email)
    {
        _currentUser = CreatePrincipal(email);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static ClaimsPrincipal CreatePrincipal(string email)
    {
        Claim[] claims = [new(ClaimTypes.Email, email), new(ClaimTypes.Name, email)];
        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }
}
