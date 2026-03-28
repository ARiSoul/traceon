namespace Traceon.Infrastructure.Email;

internal static class EmailTemplates
{
    public static string Confirmation(string url, string lang) => Wrap(
        S(lang, "confirm_heading"),
        $"""
        <p>{S(lang, "confirm_welcome")}</p>
        <p>{S(lang, "confirm_body")}</p>
        """,
        url,
        S(lang, "confirm_button"),
        S(lang, "link_fallback"),
        S(lang, "footer"));

    public static string PasswordReset(string url, string lang) => Wrap(
        S(lang, "reset_heading"),
        $"""
        <p>{S(lang, "reset_body1")}</p>
        <p>{S(lang, "reset_body2")}</p>
        """,
        url,
        S(lang, "reset_button"),
        S(lang, "link_fallback"),
        S(lang, "footer"));

    public static string Subject(string lang, string key) => S(lang, key);

    // ── Localized strings ──────────────────────────────────────────────

    private static string S(string lang, string key) =>
        Strings.TryGetValue(Normalize(lang), out var dict) && dict.TryGetValue(key, out var val)
            ? val
            : Strings["en"][key];

    private static string Normalize(string? lang) =>
        lang?.ToLowerInvariant().Split('-')[0] switch
        {
            "pt" => "pt",
            "es" => "es",
            _ => "en"
        };

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["en"] = new()
        {
            ["confirm_heading"] = "Confirm your email",
            ["confirm_welcome"] = "Welcome to <strong>Traceon</strong>!",
            ["confirm_body"] = "Please confirm your email address by clicking the button below:",
            ["confirm_button"] = "Confirm Email",
            ["confirm_subject"] = "Confirm your Traceon account",
            ["reset_heading"] = "Reset your password",
            ["reset_body1"] = "We received a request to reset your <strong>Traceon</strong> password.",
            ["reset_body2"] = "Click the button below to choose a new password. If you didn't request this, you can safely ignore this email.",
            ["reset_button"] = "Reset Password",
            ["reset_subject"] = "Reset your Traceon password",
            ["link_fallback"] = "If the button doesn't work, copy and paste this link:",
            ["footer"] = "&copy; Traceon &mdash; Action Tracking",
        },
        ["pt"] = new()
        {
            ["confirm_heading"] = "Confirme o seu email",
            ["confirm_welcome"] = "Bem-vindo ao <strong>Traceon</strong>!",
            ["confirm_body"] = "Por favor, confirme o seu endereço de email clicando no botão abaixo:",
            ["confirm_button"] = "Confirmar Email",
            ["confirm_subject"] = "Confirme a sua conta Traceon",
            ["reset_heading"] = "Redefinir a sua palavra-passe",
            ["reset_body1"] = "Recebemos um pedido para redefinir a sua palavra-passe do <strong>Traceon</strong>.",
            ["reset_body2"] = "Clique no botão abaixo para escolher uma nova palavra-passe. Se não fez este pedido, pode ignorar este email.",
            ["reset_button"] = "Redefinir Palavra-passe",
            ["reset_subject"] = "Redefinir a sua palavra-passe Traceon",
            ["link_fallback"] = "Se o botão não funcionar, copie e cole este link:",
            ["footer"] = "&copy; Traceon &mdash; Registo de Acções",
        },
        ["es"] = new()
        {
            ["confirm_heading"] = "Confirma tu email",
            ["confirm_welcome"] = "¡Bienvenido a <strong>Traceon</strong>!",
            ["confirm_body"] = "Por favor, confirma tu dirección de email haciendo clic en el botón de abajo:",
            ["confirm_button"] = "Confirmar Email",
            ["confirm_subject"] = "Confirma tu cuenta Traceon",
            ["reset_heading"] = "Restablecer tu contraseña",
            ["reset_body1"] = "Recibimos una solicitud para restablecer tu contraseña de <strong>Traceon</strong>.",
            ["reset_body2"] = "Haz clic en el botón de abajo para elegir una nueva contraseña. Si no solicitaste esto, puedes ignorar este email.",
            ["reset_button"] = "Restablecer Contraseña",
            ["reset_subject"] = "Restablecer tu contraseña Traceon",
            ["link_fallback"] = "Si el botón no funciona, copia y pega este enlace:",
            ["footer"] = "&copy; Traceon &mdash; Registro de Acciones",
        },
    };

    // ── HTML layout ────────────────────────────────────────────────────

    private static string Wrap(string heading, string body, string actionUrl, string actionText, string linkFallback, string footer) =>
        $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8"/>
            <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
        </head>
        <body style="margin:0; padding:0; background-color:#f4f4f7; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
            <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f7; padding:40px 0;">
                <tr>
                    <td align="center">
                        <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.08);">
                            <tr>
                                <td style="background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%); padding:24px 32px; text-align:center;">
                                    <span style="color:#ffffff; font-size:24px; font-weight:bold;">&#x25CE; Traceon</span>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:32px;">
                                    <h2 style="margin:0 0 16px; color:#1a1a2e; font-size:20px;">{{heading}}</h2>
                                    {{body}}
                                    <table cellpadding="0" cellspacing="0" style="margin:24px 0;">
                                        <tr>
                                            <td style="background-color:#1b6ec2; border-radius:6px;">
                                                <a href="{{actionUrl}}" style="display:inline-block; padding:12px 28px; color:#ffffff; text-decoration:none; font-weight:600; font-size:14px;">{{actionText}}</a>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style="font-size:12px; color:#999; margin-top:24px;">
                                        {{linkFallback}}<br/>
                                        <a href="{{actionUrl}}" style="color:#1b6ec2; word-break:break-all;">{{actionUrl}}</a>
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:16px 32px; background-color:#f9f9fb; text-align:center; font-size:12px; color:#999;">
                                    {{footer}}
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
}
