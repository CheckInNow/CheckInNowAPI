using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Unban;

public sealed class UnbanUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<UnbanUserCommand>
{
    public async Task<Result> Handle(UnbanUserCommand command, CancellationToken cancellationToken = default)
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

        if (user.LockoutEnd is null || user.LockoutEnd <= timeProvider.GetUtcNow())
        {
            return Result.Failure(UserErrors.NotBanned);
        }

        user.LockoutEnd = null;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.UpdateFailed(string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
