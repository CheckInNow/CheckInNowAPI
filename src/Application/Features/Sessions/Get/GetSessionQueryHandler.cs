using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Get;

public sealed class GetSessionQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetSessionQuery, SessionDetailResponse>
{
    public async Task<Result<SessionDetailResponse>> Handle(GetSessionQuery query, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.Sessions
            .Where(s => s.Id == query.SessionId
                && s.ConferenceId == query.ConferenceId
                && s.DeletedAt == null
                && s.Conference.DeletedAt == null)
            .Select(s => new SessionDetailResponse(
                s.Id,
                s.ConferenceId,
                s.Title,
                s.Description,
                s.StartTime,
                s.EndTime,
                s.RoomId,
                s.Room.Name,
                s.SpeakerId,
                s.Speaker.FirstName + " " + s.Speaker.LastName))
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return Result.Failure<SessionDetailResponse>(
                Error.NotFound("Session.NotFound", $"Session with ID '{query.SessionId}' was not found."));
        }

        return Result.Success(session);
    }
}
