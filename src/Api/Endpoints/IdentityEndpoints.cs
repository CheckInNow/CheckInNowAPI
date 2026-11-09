using Api.Extensions;
using Application.Features.Identity.ConfirmEmail;
using Application.Features.Identity.Login;
using Application.Features.Identity.Login.GoogleLogin;
using Application.Features.Identity.RefreshToken;
using Application.Features.Identity.Register;
using Application.Features.Identity.ResendVerification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity")
            .WithTags("Identity");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login with email and password");

        group.MapPost("/google", LoginGoogle)
            .WithName("LoginGoogle")
            .WithSummary("Login with Google");

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refresh an expired access token");

        group.MapPost("/confirm-email", ConfirmEmail)
            .WithName("ConfirmEmail")
            .WithSummary("Confirm email address with the token from the confirmation link")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/resend-verification", ResendVerification)
            .WithName("ResendVerification")
            .WithSummary("Resend the email confirmation link")
            .RequireRateLimiting(RateLimitPolicies.ResendVerification)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> ConfirmEmail(
        [FromBody] ConfirmEmailCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> ResendVerification(
        [FromBody] ResendVerificationCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> LoginGoogle(
        [FromBody] GoogleLoginCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
