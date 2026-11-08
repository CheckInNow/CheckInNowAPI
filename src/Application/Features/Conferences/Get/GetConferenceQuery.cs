
using Application.Abstractions.Messaging;

namespace Application.Features.Conferences.Get;

public sealed record GetConferenceQuery(Guid Id) : IQuery<ConferenceDetailResponse>;

public sealed record ConferenceDetailResponse(
    Guid Id, string Name, string? Description,
    DateTimeOffset StartDate, DateTimeOffset EndDate,
    string Location, int ParticipantLimit);
