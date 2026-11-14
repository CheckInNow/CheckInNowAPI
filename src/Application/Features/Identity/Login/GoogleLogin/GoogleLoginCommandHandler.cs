using Application.Abstractions.Identity;
using Application.Abstractions.OAuth;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Login.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IGoogleTokenVerifier google,
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
                    return Result.Failure<TokenResponse>(Error.Failure("Auth.EmailNotConfirmed", "Account email is not confirmed"));
                }

                var link = await userManager.AddLoginAsync(user, new UserLoginInfo(Provider, info.Subject, Provider));
                if (!link.Succeeded)
                {
                    return Result.Failure<TokenResponse>(Error.Failure("Auth.AddLoginFailed", "Failed to add Google login to account"));
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
                return Result.Failure<TokenResponse>(Error.Failure("Auth.CreateFailed", "Failed to create new user"));
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result.Failure<TokenResponse>(Error.Failure("Auth.RoleAssignmentFailed", "Failed to assign role."));
            }

            var link = await userManager.AddLoginAsync(user, new UserLoginInfo(Provider, info.Subject, Provider));
            if (!link.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result.Failure<TokenResponse>(Error.Failure("Auth.AddLoginFailed", "Failed to add Google login to account"));
            }
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<TokenResponse>(Error.Forbidden("Auth.LockedOut", "Account is locked out."));
        }

        var response = await tokens.GenerateTokenAsync(user, ct);
        return Result.Success(response);
    }
}
