using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Delete;

public sealed class DeleteSessionCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<DeleteSessionCommand>
{
    public async Task<Result> Handle(DeleteSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.Sessions
            .Include(s => s.Conference)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId
                && s.ConferenceId == command.ConferenceId
                && s.DeletedAt == null
                && s.Conference.DeletedAt == null, cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session.NotFound", $"Session with ID '{command.SessionId}' was not found."));
        }

        if (currentUser.UserId != session.Conference.OrganizerId)
        {
            return Result.Failure(Error.Forbidden("Session.Forbidden", "Only the conference owner can delete its sessions."));
        }

        // Soft delete
        session.DeletedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
