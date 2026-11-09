namespace Api.Requests.Conferences;

public sealed record UpdateConferenceRequest(
    string? Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string? Location,
    int? ParticipantLimit);
