using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Rooms.Get;

public sealed class GetRoomsQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetRoomsQuery, List<RoomResponse>>
{
    public async Task<Result<List<RoomResponse>>> Handle(GetRoomsQuery query, CancellationToken cancellationToken = default)
    {
        var conferenceExists = await dbContext.Conferences
            .AnyAsync(c => c.Id == query.ConferenceId && c.DeletedAt == null, cancellationToken);
        if (!conferenceExists)
        {
            return Result.Failure<List<RoomResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        var rooms = await dbContext.Rooms
            .Where(r => r.ConferenceId == query.ConferenceId)
            .OrderBy(r => r.Name)
            .Select(r => new RoomResponse(r.Id, r.Name, r.Capacity))
            .ToListAsync(cancellationToken);

        return Result.Success(rooms);
    }
}
