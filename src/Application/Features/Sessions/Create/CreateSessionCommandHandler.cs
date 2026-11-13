using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Create;

public sealed class CreateSessionCommandHandler(IAppDbContext context) : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<Result<CreateSessionResponse>> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        var info = await context.Conferences
            .Where(c => c.Id == command.ConferenceId && c.DeletedAt == null)
            .Select(c => new
            {
                ConferenceStartDate = c.StartDate,
                ConferenceEndDate = c.EndDate,
                RoomExists = c.Rooms.Any(r => r.Id == command.RoomId),
                SpeakerExists = c.Speakers.Any(s => s.Id == command.SpeakerId),
                RoomBusy = c.Sessions.Any(s =>
                    s.RoomId == command.RoomId
                    && s.DeletedAt == null
                    && command.StartTime < s.EndTime
                    && command.EndTime > s.StartTime),
                SpeakerBusy = c.Sessions.Any(s =>
                    s.SpeakerId == command.SpeakerId
                    && s.DeletedAt == null
                    && command.StartTime < s.EndTime
                    && command.EndTime > s.StartTime)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (info is null)
        {
            return Result.Failure<CreateSessionResponse>(Error.NotFound("Conference.NotFound", $"Conference with ID '{command.ConferenceId}' was not found."));
        }

        if (!info.RoomExists)
        {
            return Result.Failure<CreateSessionResponse>(Error.NotFound("Room.NotFound", $"Room with ID '{command.RoomId}' was not found."));
        }

        if (!info.SpeakerExists)
        {
            return Result.Failure<CreateSessionResponse>(Error.NotFound("Speaker.NotFound", $"Speaker with ID '{command.SpeakerId}' was not found."));
        }

        if (command.StartTime < info.ConferenceStartDate || command.EndTime > info.ConferenceEndDate)
        {
            return Result.Failure<CreateSessionResponse>(Error.Validation("Session.InvalidTime", "Session time must be within the conference time range."));
        }

        if (info.RoomBusy)
        {
            return Result.Failure<CreateSessionResponse>(Error.Validation("Session.RoomOverlapping", "Session time overlaps with an existing session in the same room."));
        }

        if (info.SpeakerBusy)
        {
            return Result.Failure<CreateSessionResponse>(Error.Validation("Session.SpeakerBusy", "Speaker already has a session at this time."));
        }

        var session = new Session
        {
            ConferenceId = command.ConferenceId,
            Title = command.Title,
            Description = command.Description ?? string.Empty,
            StartTime = command.StartTime.ToUniversalTime(),
            EndTime = command.EndTime.ToUniversalTime(),
            RoomId = command.RoomId,
            SpeakerId = command.SpeakerId
        };

        context.Sessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateSessionResponse(
            session.Id,
            session.ConferenceId,
            session.Title,
            session.Description,
            session.StartTime,
            session.EndTime,
            session.RoomId,
            session.SpeakerId
            ));
    }
}
