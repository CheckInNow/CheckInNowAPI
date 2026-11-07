namespace Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string receiver, string subject, string text, string html);
}
