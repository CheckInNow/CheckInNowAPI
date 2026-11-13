using Application.Abstractions.Messaging;

namespace Application.Features.Users.RemoveOrganizerRole;

public sealed record RemoveOrganizerRoleCommand(Guid UserId) : ICommand;
