using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Rooms.Get;

public sealed class GetRoomsQueryHandler(IAppDbContext dbContext) : IQueryHandler<GetRoomsQuery, List<RoomResponse>>
{
    public async Task<Result<List<RoomResponse>>> Handle(GetRoomsQuery query, CancellationToken cancellationToken = default)
    {
        var data = await dbContext.Conferences
            .Where(c => c.Id == query.ConferenceId && c.DeletedAt == null)
            .Select(c => new
            {
                Rooms = c.Rooms
                    .OrderBy(r => r.Name)
                    .Select(r => new RoomResponse(r.Id, r.Name, r.Capacity))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return Result.Failure<List<RoomResponse>>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{query.ConferenceId}' was not found."));
        }

        return Result.Success(data.Rooms);
    }
}
