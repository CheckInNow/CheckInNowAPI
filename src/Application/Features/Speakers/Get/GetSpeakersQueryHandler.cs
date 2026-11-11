using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Speakers.Get;

public sealed class GetSpeakersQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetSpeakersQuery, List<SpeakerResponse>>
{
    public async Task<Result<List<SpeakerResponse>>> Handle(GetSpeakersQuery query, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences
            .Where(c => c.Id == query.ConferenceId && c.DeletedAt == null)
            .Select(c => new
            {
                Speakers = c.Speakers
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Select(s => new SpeakerResponse(s.Id, s.FirstName, s.LastName, s.Bio))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (conference is null)
        {
            return Result.Failure<List<SpeakerResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        return Result.Success(conference.Speakers);
    }
}
