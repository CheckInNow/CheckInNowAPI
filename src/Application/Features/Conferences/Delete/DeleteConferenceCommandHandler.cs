using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Conferences.Delete;

public sealed class DeleteConferenceCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<DeleteConferenceCommand>
{
    public async Task<Result> Handle(DeleteConferenceCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences.FindAsync([command.Id], cancellationToken);
        if (conference is null || conference.DeletedAt is not null)
        {
            return Result.Failure(Error.NotFound("Conference.NotFound", $"Conference with ID '{command.Id}' was not found."));
        }

        if (currentUser.UserId != conference.OrganizerId)
        {
            return Result.Failure(Error.Forbidden("Conference.Forbidden", "Only the owner can delete this conference."));
        }

        // Soft delete
        conference.DeletedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
