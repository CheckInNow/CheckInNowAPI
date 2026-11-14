namespace Api.Requests.Sessions;

public sealed record CreateSessionRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid RoomId,
    Guid SpeakerId
);
