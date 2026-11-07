using Application.Abstractions.Email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Email;

public sealed class MailpitEmailSender(IConfiguration configuration) : IEmailSender
{
    private const string DefaultHost = "mailpit";
    private const int DefaultSmtpPort = 1025;

    public async Task SendEmailAsync(string receiver, string subject, string text, string html)
    {
        var message = CreateMessage(receiver, subject, text, html);

        var host = configuration["MAILPIT_HOST"] ?? DefaultHost;
        var port = int.TryParse(configuration["MAILPIT_SMTP_PORT"], out var configuredPort)
            ? configuredPort
            : DefaultSmtpPort;

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public static MimeMessage CreateMessage(string receiver, string subject, string text, string html)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Test Sender", "test@local.dev"));
        message.To.Add(new MailboxAddress(receiver, receiver));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            TextBody = text,
            HtmlBody = html
        };
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }
}
