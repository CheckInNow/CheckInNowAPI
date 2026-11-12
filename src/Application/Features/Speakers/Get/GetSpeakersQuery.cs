using Application.Abstractions.Messaging;

namespace Application.Features.Speakers.Get;

public sealed record GetSpeakersQuery(Guid ConferenceId) : IQuery<List<SpeakerResponse>>;

public sealed record SpeakerResponse(Guid Id, string FirstName, string LastName, string? Bio);
