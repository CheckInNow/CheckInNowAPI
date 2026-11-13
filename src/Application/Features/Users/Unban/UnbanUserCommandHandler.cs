using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Unban;

public sealed class UnbanUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser)
    : ICommandHandler<UnbanUserCommand>
{
    public async Task<Result> Handle(UnbanUserCommand command, CancellationToken cancellationToken = default)
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

        if (user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow)
        {
            return Result.Failure(Error.Conflict("User.NotBanned", "The user is not banned."));
        }

        user.LockoutEnd = null;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Failure("User.UpdateFailed", string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
