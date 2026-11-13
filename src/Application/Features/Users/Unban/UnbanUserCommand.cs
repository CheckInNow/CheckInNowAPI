using Application.Abstractions.Messaging;

namespace Application.Features.Users.Unban;

public sealed record UnbanUserCommand(Guid UserId) : ICommand;
