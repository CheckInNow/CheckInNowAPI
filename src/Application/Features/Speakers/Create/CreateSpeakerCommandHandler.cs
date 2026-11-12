using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;

namespace Application.Features.Speakers.Create;

public sealed class CreateSpeakerCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<CreateSpeakerCommand, CreateSpeakerResponse>
{
    public async Task<Result<CreateSpeakerResponse>> Handle(CreateSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences.FindAsync([command.ConferenceId], cancellationToken);
        if (conference is null || conference.DeletedAt is not null)
        {
            return Result.Failure<CreateSpeakerResponse>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{command.ConferenceId}' was not found."));
        }

        if (currentUser.UserId != conference.OrganizerId)
        {
            return Result.Failure<CreateSpeakerResponse>(
                Error.Forbidden("Conference.Forbidden", "Only the owner can add speakers to this conference."));
        }

        var speaker = new Speaker
        {
            ConferenceId = command.ConferenceId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Bio = command.Bio
        };

        dbContext.Speakers.Add(speaker);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new CreateSpeakerResponse(
                speaker.Id,
                speaker.ConferenceId,
                speaker.FirstName,
                speaker.LastName,
                speaker.Bio
            ));
    }
}
