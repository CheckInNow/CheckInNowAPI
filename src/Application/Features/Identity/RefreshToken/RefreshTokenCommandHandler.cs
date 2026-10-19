
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Identity.RefreshToken;
public sealed class RefreshTokenCommandHandler(ITokenService tokenService) : ICommandHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await tokenService.RefreshTokenAsync(command.AccessToken, command.RefreshToken, cancellationToken);
            return Result.Success(token);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<TokenResponse>(Error.Validation("Auth.InvalidRefreshToken", ex.Message));
        }
    }
}
