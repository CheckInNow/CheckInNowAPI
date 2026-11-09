using Application.Abstractions.Messaging;
using Application.Features.Conferences.Get;

namespace Application.Features.Conferences.Create;

public sealed record CreateConferenceCommand(
    string Name,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Location,
    int ParticipantLimit) : ICommand<ConferenceDetailResponse>;
