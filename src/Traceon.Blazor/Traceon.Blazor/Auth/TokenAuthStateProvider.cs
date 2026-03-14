using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Traceon.Blazor.Auth;

public sealed class TokenAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    private const string TokenKey = "traceon_access_token";

    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser.Identity?.IsAuthenticated != true)
        {
            var token = await localStorage.GetItemAsStringAsync(TokenKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                var identity = ParseToken(token);

                if (identity.IsAuthenticated)
                    _currentUser = new ClaimsPrincipal(identity);
            }
        }

        return new AuthenticationState(_currentUser);
    }

    public void NotifyUserAuthentication(string token)
    {
        var identity = ParseToken(token);
        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static ClaimsIdentity ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            return new ClaimsIdentity();

        var jwt = handler.ReadJwtToken(token);
        return new ClaimsIdentity(jwt.Claims, "Bearer");
    }
}
