using Application.Abstractions.Messaging;

namespace Application.Features.Sessions.Get;

public sealed record GetSessionQuery(Guid ConferenceId, Guid SessionId) : IQuery<SessionDetailResponse>;

public sealed record SessionDetailResponse(
    Guid Id,
    Guid ConferenceId,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid RoomId,
    string RoomName,
    Guid SpeakerId,
    string SpeakerName);
