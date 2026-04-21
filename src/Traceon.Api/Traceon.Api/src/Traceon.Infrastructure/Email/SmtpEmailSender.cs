using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MimeKit;
using Traceon.Infrastructure.Identity;

namespace Traceon.Infrastructure.Email;

internal sealed class SmtpEmailSender(EmailSettings settings, ILogger<SmtpEmailSender> logger)
    : IEmailSender<ApplicationUser>
{
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var lang = user.PreferredLanguage ?? "en";
        var url = BuildClientUrl($"/auth/confirm-email?token={confirmationLink}&email={Uri.EscapeDataString(email)}");
        var body = EmailTemplates.Confirmation(url, lang);
        var subject = EmailTemplates.Subject(lang, "confirm_subject");
        await SendAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var lang = user.PreferredLanguage ?? "en";
        var url = BuildClientUrl($"/auth/reset-password?code={resetLink}&email={Uri.EscapeDataString(email)}");
        var body = EmailTemplates.PasswordReset(url, lang);
        var subject = EmailTemplates.Subject(lang, "reset_subject");
        await SendAsync(email, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var lang = user.PreferredLanguage ?? "en";
        var url = BuildClientUrl($"/auth/reset-password?code={resetCode}&email={Uri.EscapeDataString(email)}");
        var body = EmailTemplates.PasswordReset(url, lang);
        var subject = EmailTemplates.Subject(lang, "reset_subject");
        await SendAsync(email, subject, body);
    }

    private string BuildClientUrl(string path)
    {
        var baseUrl = settings.ClientBaseUrl?.TrimEnd('/') ?? "http://localhost:5284";
        return $"{baseUrl}{path}";
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            var secureSocket = settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, secureSocket);

            if (!string.IsNullOrEmpty(settings.SmtpUser))
                await client.AuthenticateAsync(settings.SmtpUser, settings.SmtpPassword!);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }
}
