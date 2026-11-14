
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = tokenService.GetUserIdFromExpiredToken(command.AccessToken);
            var user = await userManager.FindByIdAsync(userId);
            if (user is not null && await userManager.IsLockedOutAsync(user))
            {
                return Result.Failure<TokenResponse>(Error.Failure("Auth.LockedOut", "Account is locked out"));
            }

            var token = await tokenService.RefreshTokenAsync(command.AccessToken, command.RefreshToken, cancellationToken);
            return Result.Success(token);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<TokenResponse>(Error.Validation("Auth.InvalidRefreshToken", ex.Message));
        }
    }
}
