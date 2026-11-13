using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.AddOrganizerRole;

public sealed class AddOrganizerRoleCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser)
    : ICommandHandler<AddOrganizerRoleCommand>
{
    public async Task<Result> Handle(AddOrganizerRoleCommand command, CancellationToken cancellationToken = default)
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

        if (await userManager.IsInRoleAsync(user, Roles.Organizer))
        {
            return Result.Failure(Error.Conflict("User.AlreadyOrganizer", "The user already has the Organizer role."));
        }

        var result = await userManager.AddToRoleAsync(user, Roles.Organizer);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Failure("User.UpdateFailed", string.Join(" ", result.Errors.Select(e => e.Description))));
    }
}
