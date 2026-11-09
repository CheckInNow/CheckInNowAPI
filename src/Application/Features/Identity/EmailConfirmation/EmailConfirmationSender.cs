using System.Text;
using Application.Abstractions.Email;
using Application.Abstractions.Shared;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Features.Identity.EmailConfirmation;

public sealed class EmailConfirmationSender(
    UserManager<ApplicationUser> userManager,
    IFrontendLinkBuilder linkBuilder,
    IEmailSender emailSender)
{
    public const string ConfirmEmailAction = "confirm-email";

    public async Task SendAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = linkBuilder.BuildFrontendLink(ConfirmEmailAction, user.Id, encodedToken);

        const string subject = "Confirm your email - CheckInNow";
        var text = $"Hi {user.FirstName},\n\nConfirm your email address by opening this link:\n{link}\n\nThe link expires in 24 hours.";
        var html = $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Confirm your email address by clicking the link below:</p>
            <p><a href="{link}">Confirm email</a></p>
            <p>The link expires in 24 hours.</p>
            """;

        await emailSender.SendEmailAsync(user.Email!, subject, text, html);
    }
}
