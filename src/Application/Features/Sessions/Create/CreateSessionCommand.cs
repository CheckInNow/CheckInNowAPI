using Application.Abstractions.Messaging;

namespace Application.Features.Sessions.Create;

public sealed record CreateSessionCommand(
    Guid ConferenceId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid RoomId,
    Guid SpeakerId
) : ICommand<CreateSessionResponse>;

public sealed record CreateSessionResponse(
    Guid Id,
    Guid ConferenceId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid RoomId,
    Guid SpeakerId
);
