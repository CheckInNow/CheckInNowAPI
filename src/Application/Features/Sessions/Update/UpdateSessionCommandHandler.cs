using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Update;

public sealed class UpdateSessionCommandHandler(IAppDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<UpdateSessionCommand>
{
    public async Task<Result> Handle(UpdateSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.Sessions
            .Include(s => s.Conference)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId
                && s.ConferenceId == command.ConferenceId
                && s.DeletedAt == null
                && s.Conference.DeletedAt == null, cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session.NotFound", $"Session with ID '{command.SessionId}' was not found."));
        }

        if (currentUser.UserId != session.Conference.OrganizerId)
        {
            return Result.Failure(Error.Forbidden("Session.Forbidden", "Only the conference owner can edit its sessions."));
        }

        // Validate the merged values: PATCH may change only one of the fields that depend on each other.
        var startTime = command.StartTime?.ToUniversalTime() ?? session.StartTime;
        var endTime = command.EndTime?.ToUniversalTime() ?? session.EndTime;
        var roomId = command.RoomId ?? session.RoomId;
        var speakerId = command.SpeakerId ?? session.SpeakerId;

        var timeChanged = startTime != session.StartTime || endTime != session.EndTime;
        var roomChanged = roomId != session.RoomId;
        var speakerChanged = speakerId != session.SpeakerId;

        if (startTime >= endTime)
        {
            return Result.Failure(Error.Validation("Session.InvalidTime", "End time must be after the start time."));
        }

        if (timeChanged && (startTime < session.Conference.StartDate || endTime > session.Conference.EndDate))
        {
            return Result.Failure(Error.Validation("Session.InvalidTime", "Session time must be within the conference time range."));
        }

        if (roomChanged)
        {
            var roomExists = await dbContext.Rooms
                .AnyAsync(r => r.Id == roomId && r.ConferenceId == session.ConferenceId, cancellationToken);
            if (!roomExists)
            {
                return Result.Failure(Error.NotFound("Room.NotFound", $"Room with ID '{roomId}' was not found."));
            }
        }

        if (speakerChanged)
        {
            var speakerExists = await dbContext.Speakers
                .AnyAsync(s => s.Id == speakerId && s.ConferenceId == session.ConferenceId, cancellationToken);
            if (!speakerExists)
            {
                return Result.Failure(Error.NotFound("Speaker.NotFound", $"Speaker with ID '{speakerId}' was not found."));
            }
        }

        if (timeChanged || roomChanged)
        {
            var roomBusy = await dbContext.Sessions.AnyAsync(s =>
                s.Id != session.Id
                && s.RoomId == roomId
                && s.DeletedAt == null
                && startTime < s.EndTime
                && endTime > s.StartTime, cancellationToken);
            if (roomBusy)
            {
                return Result.Failure(Error.Validation("Session.RoomOverlapping", "Session time overlaps with an existing session in the same room."));
            }
        }

        if (timeChanged || speakerChanged)
        {
            var speakerBusy = await dbContext.Sessions.AnyAsync(s =>
                s.Id != session.Id
                && s.SpeakerId == speakerId
                && s.DeletedAt == null
                && startTime < s.EndTime
                && endTime > s.StartTime, cancellationToken);
            if (speakerBusy)
            {
                return Result.Failure(Error.Validation("Session.SpeakerBusy", "Speaker already has a session at this time."));
            }
        }

        session.StartTime = startTime;
        session.EndTime = endTime;
        session.RoomId = roomId;
        session.SpeakerId = speakerId;

        if (command.Title is not null)
        {
            session.Title = command.Title;
        }

        if (command.Description is not null)
        {
            session.Description = command.Description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
