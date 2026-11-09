using Application.Abstractions.Messaging;

namespace Application.Features.Identity.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : ICommand;
