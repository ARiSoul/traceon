using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Traceon.Infrastructure.Email;
using Traceon.Infrastructure.Identity;

namespace Traceon.Api.Endpoints;

internal static class IdentityEndpoints
{
    private const string LoginProvider = "Traceon";
    private const string RefreshTokenName = "RefreshToken";

    public static RouteGroupBuilder MapIdentityEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/confirm-email", ConfirmEmailAsync).AllowAnonymous();
        group.MapPost("/resend-confirmation", ResendConfirmationAsync).AllowAnonymous();
        group.MapPost("/forgot-password", ForgotPasswordAsync).AllowAnonymous();
        group.MapPost("/reset-password", ResetPasswordAsync).AllowAnonymous();
        group.MapPost("/change-password", ChangePasswordAsync);
        group.MapDelete("/account", DeleteAccountAsync);
        group.MapGet("/preferences", GetPreferencesAsync);
        group.MapPut("/preferences", UpdatePreferencesAsync);
        group.MapGet("/external-login", ExternalLoginAsync).AllowAnonymous();
        group.MapGet("/external-callback", ExternalCallbackAsync).AllowAnonymous();
        group.MapGet("/external-providers", GetExternalProvidersAsync).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }));
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        await emailSender.SendConfirmationLinkAsync(user, request.Email, token);

        return TypedResults.Ok();
    }

    private static async Task<IResult> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return TypedResults.Unauthorized();

        var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
        var token = Encoding.UTF8.GetString(tokenBytes);
        var result = await userManager.ConfirmEmailAsync(user, token);

        return result.Succeeded ? TypedResults.Ok() : TypedResults.Unauthorized();
    }

    private static async Task<IResult> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return TypedResults.Ok(); // Don't reveal if user exists

        if (await userManager.IsEmailConfirmedAsync(user))
            return TypedResults.Ok();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        await emailSender.SendConfirmationLinkAsync(user, request.Email, token);

        return TypedResults.Ok();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return TypedResults.Unauthorized();

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return TypedResults.Problem(
                detail: "EmailNotConfirmed",
                statusCode: 403);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
            return TypedResults.Unauthorized();

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        await userManager.SetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName, refreshToken);

        return TypedResults.Ok(new AccessTokenResponse(
            "Bearer",
            accessToken,
            tokenService.AccessTokenExpirationSeconds,
            refreshToken));
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return TypedResults.Unauthorized();

        var storedToken = await userManager.GetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName);

        if (storedToken is null || storedToken != request.RefreshToken)
            return TypedResults.Unauthorized();

        // Issue a new access token but keep the same refresh token.
        // Rotating the refresh token here would invalidate all other
        // browser sessions and cause logouts after page reloads.
        var accessToken = tokenService.GenerateAccessToken(user);

        return TypedResults.Ok(new AccessTokenResponse(
            "Bearer",
            accessToken,
            tokenService.AccessTokenExpirationSeconds,
            request.RefreshToken));
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            await emailSender.SendPasswordResetCodeAsync(user, request.Email, code);
        }

        return TypedResults.Ok();
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return TypedResults.Ok();

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));
        var result = await userManager.ResetPasswordAsync(user, code, request.NewPassword);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }));
        }

        return TypedResults.Ok();
    }

    private sealed record RegisterRequest(string Email, string Password);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshTokenRequest(string Email, string RefreshToken);
    private sealed record ConfirmEmailRequest(string Email, string Token);
    private sealed record ResendConfirmationRequest(string Email);
    private sealed record ForgotPasswordRequest(string Email);
    private sealed record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);
    private sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    private sealed record AccessTokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
    private sealed record UserPreferencesResponse(string? Theme, string? Language);
    private sealed record UpdatePreferencesRequest(string? Theme, string? Language);

    private static async Task<IResult> GetPreferencesAsync(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        return TypedResults.Ok(new UserPreferencesResponse(user.PreferredTheme, user.PreferredLanguage));
    }

    private static async Task<IResult> UpdatePreferencesAsync(
        UpdatePreferencesRequest request,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        if (request.Theme is not null)
            user.PreferredTheme = request.Theme;
        if (request.Language is not null)
            user.PreferredLanguage = request.Language;
        await userManager.UpdateAsync(user);

        return TypedResults.Ok(new UserPreferencesResponse(user.PreferredTheme, user.PreferredLanguage));
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }));
        }

        return TypedResults.Ok();
    }

    private static async Task<IResult> DeleteAccountAsync(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }));
        }

        return TypedResults.NoContent();
    }

    private static IResult ExternalLoginAsync(
        string provider,
        string? returnUrl,
        ExternalAuthSettings externalAuth,
        EmailSettings emailSettings)
    {
        var clientBaseUrl = emailSettings.ClientBaseUrl?.TrimEnd('/') ?? "http://localhost:5284";
        var callbackUrl = $"/api/identity/external-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? clientBaseUrl)}";

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { ["LoginProvider"] = provider }
        };

        return TypedResults.Challenge(properties, [provider]);
    }

    private static async Task<IResult> ExternalCallbackAsync(
        string? returnUrl,
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService,
        EmailSettings emailSettings)
    {
        var clientBaseUrl = emailSettings.ClientBaseUrl?.TrimEnd('/') ?? "http://localhost:5284";
        var errorUrl = $"{clientBaseUrl}/auth/login?error=ExternalLoginFailed";

        var result = await httpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        if (!result.Succeeded)
        {
            result = await httpContext.AuthenticateAsync();
        }

        if (!result.Succeeded)
            return TypedResults.Redirect(errorUrl);

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var providerKey = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = result.Properties?.Items["LoginProvider"]
            ?? result.Principal.Identity?.AuthenticationType
            ?? "Unknown";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            return TypedResults.Redirect(errorUrl);

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return TypedResults.Redirect(errorUrl);
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        var logins = await userManager.GetLoginsAsync(user);
        if (!logins.Any(l => l.LoginProvider == provider && l.ProviderKey == providerKey))
        {
            await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName, refreshToken);

        var redirectUrl = $"{clientBaseUrl}/auth/external-callback" +
            $"?accessToken={Uri.EscapeDataString(accessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(refreshToken)}" +
            $"&expiresIn={tokenService.AccessTokenExpirationSeconds}" +
            $"&email={Uri.EscapeDataString(email)}";

        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return TypedResults.Redirect(redirectUrl);
    }

    private static IResult GetExternalProvidersAsync(ExternalAuthSettings settings)
    {
        var providers = new List<string>();
        if (!string.IsNullOrEmpty(settings.Google?.ClientId)) providers.Add("Google");
        if (!string.IsNullOrEmpty(settings.Microsoft?.ClientId)) providers.Add("Microsoft");
        return TypedResults.Ok(providers);
    }
}
