using Application.Abstractions.Messaging;

namespace Application.Features.Identity.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand;
