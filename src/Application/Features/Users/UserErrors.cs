using Domain.Common;

namespace Application.Features.Users;

internal static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.");

    public static readonly Error CannotModifySelf =
        Error.Forbidden("User.CannotModifySelf", "Administrators cannot ban themselves or change their own roles.");

    public static readonly Error AlreadyOrganizer =
        Error.Conflict("User.AlreadyOrganizer", "The user already has the Organizer role.");

    public static readonly Error NotOrganizer =
        Error.Conflict("User.NotOrganizer", "The user does not have the Organizer role.");

    public static readonly Error AlreadyBanned =
        Error.Conflict("User.AlreadyBanned", "The user is already banned.");

    public static readonly Error NotBanned =
        Error.Conflict("User.NotBanned", "The user is not banned.");

    public static Error UpdateFailed(string description) =>
        Error.Failure("User.UpdateFailed", description);
}
