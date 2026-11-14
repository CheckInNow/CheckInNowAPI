using Application.Abstractions.Messaging;

namespace Application.Features.Sessions.Update;

/// <summary>
/// PATCH semantics: null means "leave unchanged". An empty Description clears it.
/// </summary>
public sealed record UpdateSessionCommand(
    Guid ConferenceId,
    Guid SessionId,
    string? Title,
    string? Description,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    Guid? RoomId,
    Guid? SpeakerId) : ICommand;
