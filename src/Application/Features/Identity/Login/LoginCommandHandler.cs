
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Login;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : ICommandHandler<LoginCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            return Result.Failure<TokenResponse>(Error.Validation("Auth.InvalidCredentials", "Invalid email or password."));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<TokenResponse>(Error.Failure("Auth.LockedOut", "Account is locked out"));
        }

        var isValidPassword = await userManager.CheckPasswordAsync(user, command.Password);
        if (!isValidPassword)
        {
            await userManager.AccessFailedAsync(user);
            return Result.Failure<TokenResponse>(Error.Validation("Auth.InvalidCredentials", "Invalid email or password."));
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var token = await tokenService.GenerateTokenAsync(user, cancellationToken);
        return Result.Success(token);
    }
}
