namespace Api.Requests.Sessions;

public sealed record UpdateSessionRequest(
    string? Title,
    string? Description,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    Guid? RoomId,
    Guid? SpeakerId);
