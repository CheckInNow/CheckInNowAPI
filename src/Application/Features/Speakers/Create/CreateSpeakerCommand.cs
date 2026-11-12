using Application.Abstractions.Messaging;

namespace Application.Features.Speakers.Create;

public sealed record CreateSpeakerCommand(Guid ConferenceId, string FirstName, string LastName, string? Bio)
    : ICommand<CreateSpeakerResponse>;

public sealed record CreateSpeakerResponse(Guid Id, Guid ConferenceId, string FirstName, string LastName, string? Bio);
