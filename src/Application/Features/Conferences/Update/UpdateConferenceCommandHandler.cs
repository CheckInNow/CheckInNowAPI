using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Conferences.Update;

public sealed class UpdateConferenceCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<UpdateConferenceCommand>
{
    public async Task<Result> Handle(UpdateConferenceCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences.FindAsync([command.Id], cancellationToken);
        if (conference is null || conference.DeletedAt is not null)
        {
            return Result.Failure(Error.NotFound("Conference.NotFound", $"Conference with ID '{command.Id}' was not found."));
        }

        if (currentUser.UserId != conference.OrganizerId)
        {
            return Result.Failure(Error.Forbidden("Conference.Forbidden", "Only the owner can edit this conference."));
        }

        var startDate = command.StartDate?.ToUniversalTime() ?? conference.StartDate;
        var endDate = command.EndDate?.ToUniversalTime() ?? conference.EndDate;
        if (startDate > endDate)
        {
            return Result.Failure(Error.Validation("Conference.InvalidDates", "End date must be on or after the start date."));
        }

        conference.StartDate = startDate;
        conference.EndDate = endDate;

        if (command.Name is not null)
        {
            conference.Name = command.Name;
        }

        if (command.Description is not null)
        {
            conference.Description = command.Description;
        }

        if (command.Location is not null)
        {
            conference.Location = command.Location;
        }

        if (command.ParticipantLimit is not null)
        {
            conference.ParticipantLimit = command.ParticipantLimit.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
