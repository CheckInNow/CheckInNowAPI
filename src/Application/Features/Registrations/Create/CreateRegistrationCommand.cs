using Application.Abstractions.Messaging;

namespace Application.Features.Registrations.Create;

public sealed record CreateRegistrationCommand(Guid ConferenceId) : ICommand<CreateRegistrationResponse>;

public sealed record CreateRegistrationResponse(Guid Id, Guid ConferenceId, Guid UserId, DateTimeOffset CreatedAt);
