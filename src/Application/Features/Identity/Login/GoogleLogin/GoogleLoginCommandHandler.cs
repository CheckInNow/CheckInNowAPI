using Application.Abstractions.Identity;
using Application.Abstractions.OAuth;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Login.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IGoogleTokenValidator google,
    UserManager<ApplicationUser> userManager,
    ITokenService tokens) : IRequestHandler<GoogleLoginCommand, Result<TokenResponse>>
{
    private const string Provider = "Google";

    public async Task<Result<TokenResponse>> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var info = await google.ValidateAsync(request.IdToken, ct);
        if (info is null)
        {
            return Result.Failure<TokenResponse>(Error.Failure("OAuth.InvalidToken", "Invalid Google token"));
        }

        if (!info.EmailVerified)
        {
            return Result.Failure<TokenResponse>(Error.Failure("OAuth.GoogleEmailNotVerified", "Google email is not verified"));
        }

        var user = await userManager.FindByLoginAsync(Provider, info.Subject);

        if (user is null)
        {
            user = await userManager.FindByEmailAsync(info.Email);

            if (user is not null)
            {
                if (!user.EmailConfirmed)
                {
                    return Result.Failure<TokenResponse>(Error.Failure("Account.EmailNotConfirmed", "Account email is not confirmed"));
                }

                var link = await userManager.AddLoginAsync(user, new UserLoginInfo(Provider, info.Subject, Provider));
                if (!link.Succeeded)
                {
                    return Result.Failure<TokenResponse>(Error.Failure("Account.AddLoginFailed", "Failed to add Google login to account"));
                }
            }
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = info.Email,
                Email = info.Email,
                EmailConfirmed = info.EmailVerified,
                FirstName = info.GivenName,
                LastName = info.FamilyName ?? string.Empty,
            };

            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
            {
                return Result.Failure<TokenResponse>(Error.Failure("Account.CreateFailed", "Failed to create new user"));
            }

            var link = await userManager.AddLoginAsync(user, new UserLoginInfo(Provider, info.Subject, Provider));
            if (!link.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result.Failure<TokenResponse>(Error.Failure("Account.AddLoginFailed", "Failed to add Google login to account"));
            }
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<TokenResponse>(Error.Failure("Account.LockedOut", "Account is locked out"));
        }

        var response = await tokens.GenerateTokenAsync(user, ct);
        return Result.Success(response);
    }
}
