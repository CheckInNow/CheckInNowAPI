using Application.Abstractions.Messaging;

namespace Application.Features.Conferences.Delete;

public sealed record DeleteConferenceCommand(Guid Id) : ICommand;
