using Application.Abstractions.Messaging;

namespace Application.Features.Registrations.Delete;

public sealed record DeleteRegistrationCommand(Guid ConferenceId, Guid RegistrationId) : ICommand;
