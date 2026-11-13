using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Ban;

public sealed class BanUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<BanUserCommand>
{
    public async Task<Result> Handle(BanUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (user.Id == currentUser.UserId)
        {
            return Result.Failure(UserErrors.CannotModifySelf);
        }

        if (user.LockoutEnabled && user.LockoutEnd > timeProvider.GetUtcNow())
        {
            return Result.Failure(UserErrors.AlreadyBanned);
        }

        // Account-level ban: permanent Identity lockout. No data is deleted.
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        // Revoke the refresh token so the banned user cannot renew the session.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.UpdateFailed(string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
