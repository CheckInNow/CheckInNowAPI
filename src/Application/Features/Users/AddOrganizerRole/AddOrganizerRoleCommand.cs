using Application.Abstractions.Messaging;

namespace Application.Features.Users.AddOrganizerRole;

public sealed record AddOrganizerRoleCommand(Guid UserId) : ICommand;
