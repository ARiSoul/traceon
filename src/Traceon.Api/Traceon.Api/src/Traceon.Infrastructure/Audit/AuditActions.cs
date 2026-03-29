namespace Traceon.Infrastructure.Audit;

public static class AuditActions
{
    public const string Register = "Register";
    public const string Login = "Login";
    public const string LoginExternal = "LoginExternal";
    public const string Logout = "Logout";
    public const string EmailConfirmed = "EmailConfirmed";
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordResetRequested = "PasswordResetRequested";
    public const string PasswordReset = "PasswordReset";
    public const string PreferencesUpdated = "PreferencesUpdated";
    public const string ExternalLoginAdded = "ExternalLoginAdded";
    public const string AccountDeleted = "AccountDeleted";
    public const string TokenRefreshed = "TokenRefreshed";
    public const string DataExported = "DataExported";
    public const string DataImported = "DataImported";
}
