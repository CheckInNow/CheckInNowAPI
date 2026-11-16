using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Registrations.Delete;

public sealed class DeleteRegistrationCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<DeleteRegistrationCommand>
{
    public async Task<Result> Handle(DeleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var registration = await dbContext.Registrations
            .Include(r => r.Conference)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId
                && r.ConferenceId == command.ConferenceId
                && r.Conference.DeletedAt == null, cancellationToken);

        if (registration is null)
        {
            return Result.Failure(Error.NotFound("Registration.NotFound", $"Registration with ID '{command.RegistrationId}' was not found."));
        }

        if (currentUser.UserId != registration.UserId)
        {
            return Result.Failure(Error.Forbidden("Registration.Forbidden", "Only the registered user can cancel this registration."));
        }

        if (registration.Conference.StartDate <= DateTimeOffset.UtcNow)
        {
            return Result.Failure(Error.Conflict("Registration.ConferenceStarted", "Registration cannot be cancelled after the conference has started."));
        }

        // Hard delete
        dbContext.Registrations.Remove(registration);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
