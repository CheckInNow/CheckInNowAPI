using Application.Abstractions.Messaging;

namespace Application.Features.Sessions.Delete;

public sealed record DeleteSessionCommand(Guid ConferenceId, Guid SessionId) : ICommand;
