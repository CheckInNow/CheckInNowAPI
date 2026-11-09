using Application.Features.Identity.EmailConfirmation;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Identity.Register;

public sealed class UserRegisteredHandler(
    UserManager<ApplicationUser> userManager,
    EmailConfirmationSender confirmationSender,
    ILogger<UserRegisteredHandler> logger) : INotificationHandler<UserRegistered>
{
    public async Task Handle(UserRegistered notification, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(notification.UserId.ToString());
        if (user is null)
        {
            logger.LogWarning("Cannot send confirmation email: user {UserId} not found", notification.UserId);
            return;
        }

        try
        {
            await confirmationSender.SendAsync(user);
        }
        catch (Exception ex)
        {
            // Registration already succeeded; the user can request a new link via resend-verification.
            logger.LogError(ex, "Failed to send confirmation email to user {UserId}", notification.UserId);
        }
    }
}
