using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;

namespace Application.Features.Identity.Login.GoogleLogin;

public sealed record GoogleLoginCommand(string IdToken) : ICommand<TokenResponse>;

