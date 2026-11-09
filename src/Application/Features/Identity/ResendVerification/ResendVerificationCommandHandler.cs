using Application.Abstractions.Messaging;
using Application.Features.Identity.EmailConfirmation;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.ResendVerification;

public sealed class ResendVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    EmailConfirmationSender confirmationSender) : ICommandHandler<ResendVerificationCommand>
{
    public async Task<Result> Handle(ResendVerificationCommand command, CancellationToken cancellationToken = default)
    {
        // Always succeeds so the endpoint does not reveal whether an account exists.
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || user.EmailConfirmed)
        {
            return Result.Success();
        }

        await confirmationSender.SendAsync(user);
        return Result.Success();
    }
}
