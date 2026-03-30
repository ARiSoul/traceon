namespace Traceon.Blazor.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Email, string Password);
public sealed record RefreshRequest(string Email, string RefreshToken);
public sealed record AccessTokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);
public sealed record ConfirmEmailRequest(string Email, string Token);
public sealed record ResendConfirmationRequest(string Email);
public sealed record LinkedLoginInfo(string Provider, string DisplayName);
public sealed record LinkedLoginsResponse(IReadOnlyList<LinkedLoginInfo> Logins, bool HasPassword);
