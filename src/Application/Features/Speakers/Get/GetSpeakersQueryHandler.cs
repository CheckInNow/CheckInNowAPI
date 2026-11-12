using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Speakers.Get;

public sealed class GetSpeakersQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetSpeakersQuery, List<SpeakerResponse>>
{
    public async Task<Result<List<SpeakerResponse>>> Handle(GetSpeakersQuery query, CancellationToken cancellationToken = default)
    {
        var conferenceExists = await dbContext.Conferences
            .AnyAsync(c => c.Id == query.ConferenceId && c.DeletedAt == null, cancellationToken);
        if (!conferenceExists)
        {
            return Result.Failure<List<SpeakerResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        var speakers = await dbContext.Speakers
            .Where(s => s.ConferenceId == query.ConferenceId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Select(s => new SpeakerResponse(s.Id, s.FirstName, s.LastName, s.Bio))
            .ToListAsync(cancellationToken);

        return Result.Success(speakers);
    }
}
