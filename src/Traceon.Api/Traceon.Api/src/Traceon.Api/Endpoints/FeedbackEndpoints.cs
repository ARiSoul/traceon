using System.Security.Claims;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using Traceon.Infrastructure.Audit;
using Traceon.Infrastructure.Email;
using Traceon.Infrastructure.Identity;

namespace Traceon.Api.Endpoints;

internal static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/feedback", SendFeedbackAsync)
            .RequireAuthorization()
            .WithTags("Feedback");

        return app;
    }

    private static async Task<IResult> SendFeedbackAsync(
        FeedbackRequest request,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        EmailSettings emailSettings,
        AuditService audit,
        ILogger<Program> logger)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return TypedResults.BadRequest("Message is required.");

        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        var category = request.Category ?? "General";
        var subject = $"[Traceon Feedback] [{category}] from {user.Email}";

        var html = $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px;">
                <h2 style="color: #0d6efd;">Traceon Feedback</h2>
                <table style="border-collapse: collapse; width: 100%; margin-bottom: 16px;">
                    <tr><td style="padding: 8px; font-weight: bold; color: #6c757d;">From</td><td style="padding: 8px;">{user.Email}</td></tr>
                    <tr><td style="padding: 8px; font-weight: bold; color: #6c757d;">Category</td><td style="padding: 8px;">{category}</td></tr>
                    <tr><td style="padding: 8px; font-weight: bold; color: #6c757d;">User ID</td><td style="padding: 8px; font-family: monospace; font-size: 0.85em;">{userId}</td></tr>
                </table>
                <div style="background: #f8f9fa; border-radius: 8px; padding: 16px; white-space: pre-wrap;">{System.Net.WebUtility.HtmlEncode(request.Message)}</div>
            </div>
            """;

        var feedbackTo = emailSettings.FeedbackEmail;

        if (string.IsNullOrEmpty(feedbackTo))
        {
            logger.LogWarning("FeedbackEmail not configured. Feedback from {Email} discarded.", user.Email);
            return TypedResults.Ok();
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings.FromName, emailSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(feedbackTo));
            message.ReplyTo.Add(MailboxAddress.Parse(user.Email!));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = html };

            using var client = new SmtpClient();
            var secureSocket = emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(emailSettings.SmtpHost, emailSettings.SmtpPort, secureSocket);

            if (!string.IsNullOrEmpty(emailSettings.SmtpUser))
                await client.AuthenticateAsync(emailSettings.SmtpUser, emailSettings.SmtpPassword);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send feedback email from {Email}", user.Email);
            return TypedResults.Problem("Failed to send feedback. Please try again later.", statusCode: 500);
        }

        await audit.LogAsync(userId, user.Email!, AuditActions.FeedbackSent, new { category });

        return TypedResults.Ok();
    }

    private sealed record FeedbackRequest(string? Category, string Message);
}
