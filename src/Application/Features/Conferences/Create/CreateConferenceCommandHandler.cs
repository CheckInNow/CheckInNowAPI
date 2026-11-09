using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Application.Features.Conferences.Get;
using Domain.Common;
using Domain.Entities;

namespace Application.Features.Conferences.Create;

public sealed class CreateConferenceCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<CreateConferenceCommand, ConferenceDetailResponse>
{
    public async Task<Result<ConferenceDetailResponse>> Handle(CreateConferenceCommand command, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<ConferenceDetailResponse>(Error.Forbidden("Auth.Forbidden", "Current user could not be determined."));
        }

        var conference = new Conference
        {
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            StartDate = command.StartDate.ToUniversalTime(),
            EndDate = command.EndDate.ToUniversalTime(),
            Location = command.Location,
            ParticipantLimit = command.ParticipantLimit,
            OrganizerId = currentUser.UserId.Value
        };

        dbContext.Conferences.Add(conference);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new ConferenceDetailResponse(
            conference.Id, conference.Name, conference.Description,
            conference.StartDate, conference.EndDate,
            conference.Location, conference.ParticipantLimit));
    }
}
