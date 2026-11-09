using System.Text;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Features.Identity.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager) : ICommandHandler<ConfirmEmailCommand>
{
    private static readonly Error InvalidToken =
        Error.Validation("Auth.InvalidConfirmationToken", "The confirmation link is invalid or has expired.");

    public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(InvalidToken);
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        }
        catch (FormatException)
        {
            return Result.Failure(InvalidToken);
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Result.Success() : Result.Failure(InvalidToken);
    }
}
