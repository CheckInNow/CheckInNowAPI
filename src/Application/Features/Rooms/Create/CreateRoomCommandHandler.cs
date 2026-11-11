using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Rooms.Create;

public sealed class CreateRoomCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<CreateRoomCommand, CreateRoomResponse>
{
    public async Task<Result<CreateRoomResponse>> Handle(CreateRoomCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await dbContext.Conferences.FindAsync([command.ConferenceId], cancellationToken);
        if (conference is null || conference.DeletedAt is not null)
        {
            return Result.Failure<CreateRoomResponse>(
                Error.NotFound("Conference.NotFound", $"Conference with ID '{command.ConferenceId}' was not found."));
        }

        if (currentUser.UserId != conference.OrganizerId)
        {
            return Result.Failure<CreateRoomResponse>(
                Error.Forbidden("Conference.Forbidden", "Only the owner can add rooms to this conference."));
        }

        var nameTaken = await dbContext.Rooms
            .AnyAsync(r => r.ConferenceId == command.ConferenceId && r.Name == command.Name, cancellationToken);
        if (nameTaken)
        {
            return Result.Failure<CreateRoomResponse>(
                Error.Conflict("Room.NameTaken", $"A room named '{command.Name}' already exists in this conference."));
        }

        var room = new Room
        {
            ConferenceId = command.ConferenceId,
            Name = command.Name,
            Capacity = command.Capacity
        };

        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateRoomResponse(room.Id, room.ConferenceId, room.Name, room.Capacity));
    }
}
