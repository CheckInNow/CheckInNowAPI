using Application.Abstractions.Messaging;

namespace Application.Features.Users.Ban;

public sealed record BanUserCommand(Guid UserId) : ICommand;
