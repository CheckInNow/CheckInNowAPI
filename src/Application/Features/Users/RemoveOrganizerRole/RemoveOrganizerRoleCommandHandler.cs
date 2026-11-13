using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.RemoveOrganizerRole;

public sealed class RemoveOrganizerRoleCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser)
    : ICommandHandler<RemoveOrganizerRoleCommand>
{
    public async Task<Result> Handle(RemoveOrganizerRoleCommand command, CancellationToken cancellationToken = default)
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

        if (!await userManager.IsInRoleAsync(user, Roles.Organizer))
        {
            return Result.Failure(UserErrors.NotOrganizer);
        }

        var result = await userManager.RemoveFromRoleAsync(user, Roles.Organizer);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.UpdateFailed(string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
