using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Sessions.GetAll;

public sealed record GetAllSessionsQuery(Guid ConferenceId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResult<SessionListItemResponse>>;

public sealed record SessionListItemResponse(
    Guid Id,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid RoomId,
    string RoomName,
    Guid SpeakerId,
    string SpeakerName);
