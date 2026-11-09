using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Conferences.Get;

public sealed class GetConferenceQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetConferenceQuery, ConferenceDetailResponse>
{
    public async Task<Result<ConferenceDetailResponse>> Handle(GetConferenceQuery request, CancellationToken cancellationToken)
    {
        var conference = await dbContext.Conferences.FindAsync([request.Id], cancellationToken);
        if (conference is null || conference.DeletedAt != null)
        {
            return Result.Failure<ConferenceDetailResponse>(Error.NotFound("Conference.NotFound", $"Conference with ID '{request.Id}' was not found."));
        }

        var response = new ConferenceDetailResponse(conference.Id, conference.Name, conference.Description, conference.StartDate, conference.EndDate, conference.Location, conference.ParticipantLimit);
        return Result.Success(response);
    }
}
