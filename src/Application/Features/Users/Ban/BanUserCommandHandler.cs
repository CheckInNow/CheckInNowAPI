using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Ban;

public sealed class BanUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser)
    : ICommandHandler<BanUserCommand>
{
    public async Task<Result> Handle(BanUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", $"User with ID '{command.UserId}' was not found."));
        }

        if (user.Id == currentUser.UserId)
        {
            return Result.Failure(Error.Forbidden("User.CannotModifySelf", "Administrators cannot ban themselves or change their own roles."));
        }

        // Only a permanent lockout is a ban. A temporary lockout after failed logins is overwritten.
        if (user.LockoutEnd == DateTimeOffset.MaxValue)
        {
            return Result.Failure(Error.Conflict("User.AlreadyBanned", "The user is already banned."));
        }

        // Account-level ban: permanent Identity lockout. No data is deleted.
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.AccessFailedCount = 0;

        // Revoke the refresh token so the banned user cannot renew the session.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Failure("User.UpdateFailed", string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
