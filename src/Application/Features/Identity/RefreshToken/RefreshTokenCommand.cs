
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Identity.RefreshToken;
public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<TokenResponse>;
