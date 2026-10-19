
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Identity.Login;
public sealed record LoginCommand(string Email, string Password) : ICommand<Result<TokenResponse>>;
