using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.GetAll;

public sealed class GetAllSessionsQueryHandler(IAppDbContext dbContext)
    : IQueryHandler<GetAllSessionsQuery, PagedResult<SessionListItemResponse>>
{
    public async Task<Result<PagedResult<SessionListItemResponse>>> Handle(GetAllSessionsQuery query, CancellationToken cancellationToken = default)
    {
        var conferenceExists = await dbContext.Conferences
            .AnyAsync(c => c.Id == query.ConferenceId && c.DeletedAt == null, cancellationToken);
        if (!conferenceExists)
        {
            return Result.Failure<PagedResult<SessionListItemResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        var sessions = dbContext.Sessions
            .Where(s => s.ConferenceId == query.ConferenceId && s.DeletedAt == null);

        var totalCount = await sessions.CountAsync(cancellationToken);

        var items = await sessions
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Room.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new SessionListItemResponse(
                s.Id,
                s.Title,
                s.StartTime,
                s.EndTime,
                s.RoomId,
                s.Room.Name,
                s.SpeakerId,
                s.Speaker.FirstName + " " + s.Speaker.LastName))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<SessionListItemResponse>(items, totalCount, query.Page, query.PageSize));
    }
}
