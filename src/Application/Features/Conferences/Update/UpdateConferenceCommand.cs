using Application.Abstractions.Messaging;

namespace Application.Features.Conferences.Update;

/// <summary>
/// PATCH semantics: null means "leave unchanged". An empty Description clears it.
/// </summary>
public sealed record UpdateConferenceCommand(
    Guid Id,
    string? Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string? Location,
    int? ParticipantLimit) : ICommand;
