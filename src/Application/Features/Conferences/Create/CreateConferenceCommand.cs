using Application.Abstractions.Messaging;

namespace Application.Features.Conferences.Create;

public sealed record CreateConferenceCommand(
    string Name,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Location,
    int ParticipantLimit) : ICommand<CreateConferenceResponse>;

public sealed record CreateConferenceResponse(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Location,
    int ParticipantLimit
    );
